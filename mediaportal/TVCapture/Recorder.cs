using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using DirectX.Capture;
namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
  public class Recorder
  {
    enum State
    {
      Idle,
      Running,
      Stopping
    };

    static Thread        workerThread =null;
    static State         m_eState=State.Idle;         // thread state
    static bool          m_bRecordingsChanged=false;  // flag indicating that recordings have been added/changed/removed
    static ArrayList     m_tvcards=new ArrayList();
    static string        m_strPreviewChannel;
    static bool          m_bPreviewing=false;
    static bool          m_bPreviewChanged=false;
    static bool          m_bStopRecording=false;

    public Recorder()
    {
    }

    /// <summary>
    /// Start record thread. The recorder thread will take care of all scheduled recordings
    /// </summary>
    static public void Start()
    {
      if (m_eState!=State.Idle) Stop();
      TVDatabase.OnRecordingsChanged += new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      workerThread =new Thread( new ThreadStart(ThreadFunctionRecord)); 
      workerThread.Start();
    }

    /// <summary>
    /// Stop the record thread
    /// </summary>
    static public void Stop()
    {
      if (m_eState != State.Running) return;
      TVDatabase.OnRecordingsChanged -= new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      m_eState =State.Stopping;
      while(m_eState ==State.Stopping) System.Threading.Thread.Sleep(100);
      m_eState =State.Idle;
    }
    
    /// <summary>
    /// This function gets called by the TVDatabase when a recording has been
    /// added,changed or deleted. It forces the recorder to get the new
    /// recordings.
    /// </summary>
    static public void OnRecordingsChanged()
    {
      m_bRecordingsChanged=true;
    }
		
    /// <summary>
    /// The recorder worker thread
    /// </summary>
    static void ThreadFunctionRecord()
    {
      Log.Write("Recording thread starting");
      GUIPropertyManager.Properties["#TV.View.channel"]="";
      GUIPropertyManager.Properties["#TV.View.thumb"]  ="";
      GUIPropertyManager.Properties["#TV.View.start"]="";
      GUIPropertyManager.Properties["#TV.View.stop"] ="";
      GUIPropertyManager.Properties["#TV.View.genre"]="";
      GUIPropertyManager.Properties["#TV.View.title"]="";
      GUIPropertyManager.Properties["#TV.View.description"]="";

      GUIPropertyManager.Properties["#TV.Record.channel"]="";
      GUIPropertyManager.Properties["#TV.Record.start"]="";
      GUIPropertyManager.Properties["#TV.Record.stop"] ="";
      GUIPropertyManager.Properties["#TV.Record.genre"]="";
      GUIPropertyManager.Properties["#TV.Record.title"]="";
      GUIPropertyManager.Properties["#TV.Record.description"]="";
      GUIPropertyManager.Properties["#TV.Record.thumb"]  ="";

      System.Threading.Thread.Sleep(3000);
      m_tvcards.Clear();
      
      try
      {
        using (Stream r = File.Open(@"capturecards.xml", FileMode.Open, FileAccess.Read))
        {
          SoapFormatter c = new SoapFormatter();
          m_tvcards = (ArrayList)c.Deserialize(r);
          r.Close();
        } 
      }
      catch(Exception)
      {
      }
      if (m_tvcards.Count==0) 
      {
        Log.Write("Recorder: no capture cards found. Use file->setup to setup tvcapture!");
        m_eState=State.Idle;
      }
      for (int i=0; i < m_tvcards.Count;i++)
      {
        TVCaptureDevice card=(TVCaptureDevice)m_tvcards[i];
        card.ID=(i+1);
      }

      DateTime dtTime=DateTime.Now;

      Log.Write("Recorder: thread started. Found:{0} capture cards", m_tvcards.Count);
      foreach (TVCaptureDevice card in m_tvcards)
      {
        Log.Write(" card:{0} video device:{1} audiodevice:{2} audiocompressor:{3}  videocompressor:{4} TV:{5}  record:{6}  format:{7}",
          card.ID,card.VideoDevice,card.AudioDevice,card.AudioCompressor,card.VideoCompressor, card.UseForTV,card.UseForRecording,card.CaptureFormat);
      }
      m_eState =State.Running;
      m_bRecordingsChanged=true;
      System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.Normal;
      

      // get all TV-channels
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      
      ArrayList recordings = new ArrayList();

      int iPreRecordInterval =0;
      int iPostRecordInterval=0;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        iPreRecordInterval =xmlreader.GetValueAsInt("capture","prerecord", 5);
        iPostRecordInterval=xmlreader.GetValueAsInt("capture","postrecord", 5);
      }

      while (m_eState ==State.Running && GUIGraphicsContext.CurrentState!=GUIGraphicsContext.State.STOPPING)
      {
        try
        {

          if (m_tvcards.Count!=0) HandlePreview();

          HandleRecordings(DateTime.Now,channels,recordings,iPreRecordInterval,iPostRecordInterval);


          // wait for the next minute
          TimeSpan ts=DateTime.Now-dtTime;
          while (ts.Minutes==0 && DateTime.Now.Minute==dtTime.Minute)
          {
            if (m_eState !=State.Running) break;
            if (m_bPreviewChanged) break;
            if (m_bRecordingsChanged) break;
            if (m_bStopRecording) break;
            if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) break;
            System.Threading.Thread.Sleep(500);
            ts=DateTime.Now-dtTime;
            Process();
          }
          dtTime=DateTime.Now;
        }
        catch (Exception ex)
        {
          Log.Write("Record:exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }
      }

      foreach (TVCaptureDevice cap in m_tvcards)
      {
        if (cap.Previewing)
        {
          cap.Previewing=false;
        }
        if (cap.IsRecording)
        {
          cap.StopRecording();
        }
        cap.Process();cap.Process();cap.Process();
      }
      m_eState=State.Idle;
      Log.Write("Recorder: thread stopped");
    }

    static void HandleRecordings(DateTime dtCurrentTime, ArrayList channels, ArrayList recordings,int iPreRecordInterval,int iPostRecordInterval)
    {
      
      // If the recording schedules have been changed since last time
      if (m_bRecordingsChanged)
      {
        // then get (refresh) all recordings from the database
        recordings.Clear();
        channels.Clear();
        TVDatabase.GetRecordings(ref recordings);
        TVDatabase.GetChannels(ref channels);
        m_bRecordingsChanged=false;
      }

      
      // for each tv-channel
			TVUtil tvutil = new TVUtil();
      foreach (TVChannel chan in channels)
      {
        if (chan.Name.Equals(m_strPreviewChannel))
        {
          TVProgram prog=tvutil.GetCurrentProgram(chan.Name);
          if (prog!=null)
          {
            if (!GUIPropertyManager.Properties["#TV.View.channel"].Equals(m_strPreviewChannel))
            {
              OnPreviewChannelChanged();
            }
            GUIPropertyManager.Properties["#TV.View.start"]=prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
            GUIPropertyManager.Properties["#TV.View.stop"] =prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
            GUIPropertyManager.Properties["#TV.View.genre"]=prog.Genre;
            GUIPropertyManager.Properties["#TV.View.title"]=prog.Title;
            GUIPropertyManager.Properties["#TV.View.description"]=prog.Description;
          }
          else
          {
            if (!GUIPropertyManager.Properties["#TV.View.channel"].Equals(m_strPreviewChannel))
            {
              OnPreviewChannelChanged();
            }
            GUIPropertyManager.Properties["#TV.View.start"]="";
            GUIPropertyManager.Properties["#TV.View.stop"] ="";
            GUIPropertyManager.Properties["#TV.View.genre"]="";
            GUIPropertyManager.Properties["#TV.View.title"]="";
            GUIPropertyManager.Properties["#TV.View.description"]="";
          }
        }
      }

      if (m_tvcards.Count==0)  return;

      foreach (TVChannel chan in channels)
      {
        if (m_eState !=State.Running) break;
        if (m_bPreviewChanged) break;
        if (m_bRecordingsChanged) break;
        if (m_bStopRecording) break;
        if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) break;

        // get all programs running for this TV channel
        // between  (now-4 hours) - (now+iPostRecordInterval+3 hours)

        DateTime dtStart=dtCurrentTime.AddHours(-4);
        DateTime dtEnd=dtCurrentTime.AddMinutes(iPostRecordInterval+3*60);
        long iStartTime=Utils.datetolong(dtStart);
        long iEndTime=Utils.datetolong(dtEnd);
            
        // for each TV recording scheduled
        foreach (TVRecording rec in recordings)
        {
          if (m_eState !=State.Running) break;
          if (m_bPreviewChanged) break;
          if (m_bRecordingsChanged) break;
          if (m_bStopRecording) break;
          if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) break;
      
					// check which program is running 
					TVProgram prog=tvutil.GetProgramAt(chan.Name,dtCurrentTime.AddMinutes(iPreRecordInterval) );

          // if the recording should record the tv program
          if ( rec.ShouldRecord(dtCurrentTime,prog,iPreRecordInterval, iPostRecordInterval) )
          {
            // yes, then record it
            if (Record(dtCurrentTime,rec,prog, iPreRecordInterval, iPostRecordInterval))
            {
              break;
            }
          }
        }
      }
   

      foreach (TVRecording rec in recordings)
      {
        if (m_bPreviewChanged) break;
        if (m_bRecordingsChanged) break;
        if (m_bStopRecording) break;
        if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) break;
        // 1st check if the recording itself should b recorded
        if ( rec.ShouldRecord(DateTime.Now,null,iPreRecordInterval, iPostRecordInterval) )
        {
          // yes, then record it
          if ( Record(dtCurrentTime,rec,null,iPreRecordInterval, iPostRecordInterval))
          {
            // recording it
          }
        }
      }

      // handle properties...
      if (IsRecording)
      {
        TVRecording recording = ScheduleRecording;
        TVProgram program     = ProgramRecording;
        if (program==null)
        {
          if (!GUIPropertyManager.Properties["#TV.Record.channel"].Equals(recording.Channel))
          {
            string strLogo=Utils.GetLogo(recording.Channel);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo="defaultVideoBig.png";
            }
            GUIPropertyManager.Properties["#TV.Record.thumb"]=strLogo;
          }
          GUIPropertyManager.Properties["#TV.Record.start"]=recording.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          GUIPropertyManager.Properties["#TV.Record.stop"] =recording.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          GUIPropertyManager.Properties["#TV.Record.genre"]="";
          GUIPropertyManager.Properties["#TV.Record.title"]=recording.Title;
          GUIPropertyManager.Properties["#TV.Record.description"]="";
        }
        else
        {
          if (!GUIPropertyManager.Properties["#TV.Record.channel"].Equals(program.Channel))
          {
            string strLogo=Utils.GetLogo(program.Channel);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo="defaultVideoBig.png";
            }
            GUIPropertyManager.Properties["#TV.Record.thumb"]=strLogo;
          }
          GUIPropertyManager.Properties["#TV.Record.channel"]=program.Channel;
          GUIPropertyManager.Properties["#TV.Record.start"]=program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          GUIPropertyManager.Properties["#TV.Record.stop"] =program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          GUIPropertyManager.Properties["#TV.Record.genre"]=program.Genre;
          GUIPropertyManager.Properties["#TV.Record.title"]=program.Title;
          GUIPropertyManager.Properties["#TV.Record.description"]=program.Description;
        }
      }
      else
      {
        GUIPropertyManager.Properties["#TV.Record.channel"]="";
        GUIPropertyManager.Properties["#TV.Record.start"]="";
        GUIPropertyManager.Properties["#TV.Record.stop"] ="";
        GUIPropertyManager.Properties["#TV.Record.genre"]="";
        GUIPropertyManager.Properties["#TV.Record.title"]="";
        GUIPropertyManager.Properties["#TV.Record.description"]="";
        GUIPropertyManager.Properties["#TV.Record.thumb"]  ="";
      }
    }



    /// <summary>
    /// When called this method starts an immediate recording on the channel specified
    /// It will record the next 2 hours.
    /// </summary>
    /// <param name="strChannel"></param>
    static public void RecordNow(string strChannel)
    {
      Log.Write("Recorder: record now:"+strChannel);
      // create a new recording which records the next 2 hours...
      TVRecording tmpRec = new TVRecording();
      tmpRec.Start=Utils.datetolong(DateTime.Now);
      tmpRec.End=Utils.datetolong(DateTime.Now.AddMinutes(2*60) );
      tmpRec.Channel=strChannel;
      tmpRec.Title="Manual";
      tmpRec.RecType=TVRecording.RecordingType.Once;
      
      TVDatabase.AddRecording(ref tmpRec);
    }

    static void Process()
    {
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        try
        {
          dev.Process();
        }
        catch(Exception ex)
        {
          Log.Write("Recorder.Process exception:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
        }
      }
      m_bStopRecording=false;
    }

    static bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)
    {
      // check if we're already recording this...
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.IsRecording)
        {
          if (dev.ScheduleRecording.ID==rec.ID) return false;
        }
      }

      Log.Write("Recorder: time to record a program on channel:"+rec.Channel);
      Log.Write("Recorder: find free capture card");

      // find free device for recording
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.UseForRecording)
        {
          if (!dev.Previewing && !dev.IsRecording)
          {
            Log.Write("Recorder: found capture card:{0} {1}", dev.ID, dev.VideoDevice);
            dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
            return true;
          }
        }
      }

      //hrmmm no device free, check if we can stop a preview
      Log.Write("Recorder: All capture cards busy, stop any previewing...");
      
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.UseForRecording)
        {
          if (dev.Previewing && !dev.IsRecording)
          {
            Log.Write("Recorder: capture card:{0} {1} was previewing. Now use it for recording", dev.ID,dev.VideoDevice);
            dev.Previewing=false;
            
            m_bPreviewing=false;
            
            dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
            return true;
          }
        }
      }

      // still no device found. 
      // if we skip the pre-record interval should the new recording still be started then?
      if ( rec.ShouldRecord(currentTime,currentProgram,0,0) )
      {
        // yes, then find & stop any capture running which is busy post-recording
        Log.Write("check if a capture card is post-recording");
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.UseForRecording)
          {
            if (dev.IsPostRecording)
            {
              Log.Write("Recorder: capture card:{0} {1} was post-recording. Now use it for recording new program", dev.ID,dev.VideoDevice);
              dev.StopRecording();
              dev.Process();dev.Process();dev.Process();
              dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
              return true;
            }
          }
        }
      }

      //no device free...
      Log.Write("no capture cards are available right now for recording");
      System.Threading.Thread.Sleep(1000);
      return false;
    }

    static public void StopRecording(string m_strChannel)
    {
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.IsRecording) 
        {
          Log.Write("Recorder: Stop recording on channel:{0} capture card:{1}", m_strChannel,dev.ID);
          dev.StopRecording();
        }
      }
      m_bStopRecording=true;
    }

    static public bool IsRecording
    {
      get
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return true;
        }
        return false;
      }
    }
    
    static public TVProgram ProgramRecording
    {
      get
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return dev.ProgramRecording;
        }
        return null;
      }
    }

    static public TVRecording ScheduleRecording
    {
      get
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return dev.ScheduleRecording;
        }
        return null;
      }
    }
    
    static public string TempRecordingFileName
    {
      get
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) 
          {
            return dev.TempRecordingFileName;
          }
        }
        return "";
      }
    }

    static void OnPreviewChannelChanged()
    {
      string strLogo=Utils.GetLogo(m_strPreviewChannel);
      if (!System.IO.File.Exists(strLogo))
      {
        strLogo="defaultVideoBig.png";
      }
      GUIPropertyManager.Properties["#TV.View.channel"]=m_strPreviewChannel;
      GUIPropertyManager.Properties["#TV.View.thumb"]  =strLogo;
    }

    static void OnPreviewStopped()
    {
    }
    
    static void OnPreviewStarted()
    {
      OnPreviewChannelChanged();
    }

    static void HandlePreview()
    {
      //check if we are already previewing
      if (!m_bPreviewChanged) return;
      m_bPreviewChanged=false;

      if (m_bPreviewing)
      {
        // check if we're already previewing
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.Previewing) 
          {
            //yes then just change channels
            dev.PreviewChannel=m_strPreviewChannel;

            // if that failed
            if (!dev.Previewing) 
            {
              //we're not previewing anymore!
              m_bPreviewing=false;
              OnPreviewStopped();
            }

            // else we continue previewing on the new channel
            OnPreviewChannelChanged();
            return;
          }
        }

        // find free tuner for previewing
        Log.Write("Recorder: Start preview, find free tuner");
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (!dev.IsRecording && dev.UseForTV) 
          {
            // found one. Set it to previewing
            Log.Write("Recorder: use capture card:{0} for previewing:{1}", dev.ID,m_strPreviewChannel);
            dev.PreviewChannel=m_strPreviewChannel;
            dev.Previewing=true;
            if (!dev.Previewing) 
            {
              // failed...still not previewing
              Log.Write("Recorder: preview failed");
              m_bPreviewing=false;
            }
            else
            {
              // succeeded. We're now previewing
              Log.Write("Recorder: previewing...");
              OnPreviewStarted();
            }
            return;
          }
        }

        // no free tuner right now
        // we're still not previewing
        Log.Write("Recorder: no free tuner for previewing...");
        m_bPreviewing=false;
        return;
      }
      else
      {
        // preview should be stopped
        Log.Write("Recorder: stop preview");
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.Previewing) 
          {
            // stop previewing
            dev.Previewing=false;
            OnPreviewStopped();
          }
        }
      }
    }

    static public bool Previewing
    {
      get
      {
        return m_bPreviewing;
      }
      set
      {
        if (m_bPreviewing!=value)
        {
          if (false==value) Log.Write("Recorder:previewing stop requested");
          m_bPreviewing=value;
          m_bPreviewChanged=true;
        }
      }
    }

    static public string PreviewChannel
    {
      get 
      {
        return m_strPreviewChannel;
      }
      set
      {
        if (m_strPreviewChannel!=value) 
        {
          Log.Write("Recorder:switch to channel:{0} requested", value);
          m_strPreviewChannel=value;
          m_bPreviewChanged=true;
        }
      }
    }

  }
}
