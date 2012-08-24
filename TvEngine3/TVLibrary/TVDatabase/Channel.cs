#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using Gentle.Framework;
using TvLibrary.Log;

namespace TvDatabase
{
  /// <summary>
  /// Instances of this class represent the properties and methods of a row in the table <b>Channel</b>.
  /// </summary>
  [TableName("Channel")]
  public class Channel : Persistent
  {
    private Program _currentProgram;
    private Program _nextProgram;

    private ChannelGroup _currentGroup;

    #region Members

    private bool isChanged;
    [TableColumn("idChannel", NotNull = true), PrimaryKey(AutoGenerated = true)] private int idChannel;
    [TableColumn("isRadio", NotNull = true)] private bool isRadio;
    [TableColumn("isTv", NotNull = true)] private bool isTv;
    [TableColumn("timesWatched", NotNull = true)] private int timesWatched;
    [TableColumn("totalTimeWatched", NotNull = true)] private DateTime totalTimeWatched;
    [TableColumn("grabEpg", NotNull = true)] private bool grabEpg;
    [TableColumn("lastGrabTime", NotNull = true)] private DateTime lastGrabTime;
    [TableColumn("sortOrder", NotNull = true)] private int sortOrder;
    [TableColumn("visibleInGuide", NotNull = true)] private bool visibleInGuide;
    [TableColumn("externalId", NotNull = true)] private string externalId;
    [TableColumn("displayName", NotNull = true)] private string displayName;
    [TableColumn("epgHasGaps")] private bool epgHasGaps;
    [TableColumn("channelNumber", NotNull = true)] private int channelNumber;

    #endregion

    #region Constructors

    /// <summary> 
    /// Create a new object by specifying all fields (except the auto-generated primary key field). 
    /// </summary> 
    public Channel(bool isRadio, bool isTv, int timesWatched, DateTime totalTimeWatched, bool grabEpg,
                   DateTime lastGrabTime, int sortOrder, bool visibleInGuide, string externalId,
                   string displayName, int channelNumber)
    {
      isChanged = true;
      this.isRadio = isRadio;
      this.isTv = isTv;
      this.timesWatched = timesWatched;
      this.totalTimeWatched = totalTimeWatched;
      this.grabEpg = grabEpg;
      this.lastGrabTime = lastGrabTime;
      this.sortOrder = sortOrder;
      this.visibleInGuide = visibleInGuide;
      this.externalId = externalId;
      this.displayName = displayName;
      this.channelNumber = channelNumber;
      epgHasGaps = false;
    }


    /// <summary> 
    /// Create an object from an existing row of data. This will be used by Gentle to 
    /// construct objects from retrieved rows. 
    /// </summary> 
    public Channel(int idChannel, bool isRadio, bool isTv, int timesWatched, DateTime totalTimeWatched,
                   bool grabEpg, DateTime lastGrabTime, int sortOrder, bool visibleInGuide, string externalId,
                   string displayName, int channelNumber)
    {
      this.idChannel = idChannel;
      this.isRadio = isRadio;
      this.isTv = isTv;
      this.timesWatched = timesWatched;
      this.totalTimeWatched = totalTimeWatched;
      this.grabEpg = grabEpg;
      this.lastGrabTime = lastGrabTime;
      this.sortOrder = sortOrder;
      this.visibleInGuide = visibleInGuide;
      this.externalId = externalId;
      this.displayName = displayName;
      this.channelNumber = channelNumber;
      epgHasGaps = false;
    }

    /// <summary> 
    /// Obsolete constructor. Create a new object by specifying all fields (except the auto-generated primary key field) and the channelNumber. 
    /// </summary> 
    [System.Obsolete("Use the constructor with channelNumber")]
    public Channel(bool isRadio, bool isTv, int timesWatched, DateTime totalTimeWatched, bool grabEpg,
                   DateTime lastGrabTime, int sortOrder, bool visibleInGuide, string externalId,
                   string displayName) : this(isRadio, isTv, timesWatched, totalTimeWatched, grabEpg,
           lastGrabTime, sortOrder, visibleInGuide, externalId,
           displayName, 10000) {

    }



    #endregion

    #region Public Properties

    /// <summary>
    /// Property describing the current group that was used to view the channel from
    /// </summary>
    public ChannelGroup CurrentGroup
    {
      get { return _currentGroup; }
      set { _currentGroup = value; }
    }

    /// <summary>
    /// Indicates whether the entity is changed and requires saving or not.
    /// </summary>
    public bool IsChanged
    {
      get { return isChanged; }
    }

    /// <summary>
    /// Property relating to database column idChannel
    /// </summary>
    public int IdChannel
    {
      get { return idChannel; }
    }

    /// <summary>
    /// Property relating to database column displayName
    /// </summary>
    public string DisplayName
    {
      get { return displayName; }
      set
      {
        isChanged |= displayName != value;
        displayName = value;
      }
    }

    /// <summary>
    /// Property relating to database column name
    /// </summary>
    public string ExternalId
    {
      get { return externalId; }
      set
      {
        isChanged |= externalId != value;
        externalId = value;
      }
    }

    /// <summary>
    /// Property relating to database column isRadio
    /// </summary>
    public bool IsRadio
    {
      get { return isRadio; }
      set
      {
        isChanged |= isRadio != value;
        isRadio = value;
      }
    }

    /// <summary>
    /// Property relating to database column isTv
    /// </summary>
    public bool IsTv
    {
      get { return isTv; }
      set
      {
        isChanged |= isTv != value;
        isTv = value;
      }
    }

    /// <summary>
    /// Property relating to database column timesWatched
    /// </summary>
    public int TimesWatched
    {
      get { return timesWatched; }
      set
      {
        isChanged |= timesWatched != value;
        timesWatched = value;
      }
    }

    /// <summary>
    /// Property relating to database column totalTimeWatched
    /// </summary>
    public DateTime TotalTimeWatched
    {
      get { return totalTimeWatched; }
      set
      {
        isChanged |= totalTimeWatched != value;
        totalTimeWatched = value;
      }
    }

    /// <summary>
    /// Property relating to database column grabEpg
    /// </summary>
    public bool GrabEpg
    {
      get { return grabEpg; }
      set
      {
        isChanged |= grabEpg != value;
        grabEpg = value;
      }
    }

    /// <summary>
    /// Property relating to database column lastGrabTime
    /// </summary>
    public DateTime LastGrabTime
    {
      get { return lastGrabTime; }
      set
      {
        isChanged |= lastGrabTime != value;
        lastGrabTime = value;
      }
    }

    /// <summary>
    /// Property relating to database column sortOrder
    /// </summary>
    public int SortOrder
    {
      get { return sortOrder; }
      set
      {
        isChanged |= sortOrder != value;
        sortOrder = value;
      }
    }

    /// <summary>
    /// Property relating to database column visibleInGuide
    /// </summary>
    public bool VisibleInGuide
    {
      get { return visibleInGuide; }
      set
      {
        isChanged |= visibleInGuide != value;
        visibleInGuide = value;
      }
    }

    /// <summary>
    /// Property relating to database column epgHasGaps
    /// </summary>
    public bool EpgHasGaps
    {
      get { return epgHasGaps; }
      set
      {
        isChanged |= epgHasGaps != value;
        epgHasGaps = value;
      }
    }

    public int ChannelNumber
    {
      get { return channelNumber; }
      set
      {
        isChanged |= channelNumber != value;
        channelNumber = value;
      }
    }

    /// <summary>
    /// Gets the names of the groups where the channel is currently assigned to
    /// </summary>
    public IList<string> GroupNames
    {
      get
      {
        List<string> groupNames = new List<string>();

        if (this.IsTv)
        {
          SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (GroupMap));
          sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

          SqlStatement stmt = sb.GetStatement(true);

          IList<GroupMap> groupMaps = ObjectFactory.GetCollection<GroupMap>(stmt.Execute());

          foreach (GroupMap groupMap in groupMaps)
          {
            sb = new SqlBuilder(StatementType.Select, typeof (ChannelGroup));
            sb.AddConstraint(Operator.Equals, "idGroup", groupMap.IdGroup);

            stmt = sb.GetStatement();

            try
            {
              ChannelGroup channelGroup = ObjectFactory.GetInstance<ChannelGroup>(stmt.Execute());
              groupNames.Add(channelGroup.GroupName);
            }
            catch (Exception ex)
            {
              Log.Error("channelgroup for channel id={0} with channelgroup id={1} does not exist", idChannel,
                        groupMap.IdGroup);
            }
          }
        }
        else if (this.IsRadio)
        {
          SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (RadioGroupMap));
          sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

          SqlStatement stmt = sb.GetStatement(true);

          IList<RadioGroupMap> groupMaps = ObjectFactory.GetCollection<RadioGroupMap>(stmt.Execute());

          foreach (RadioGroupMap groupMap in groupMaps)
          {
            sb = new SqlBuilder(StatementType.Select, typeof (RadioChannelGroup));
            sb.AddConstraint(Operator.Equals, "idGroup", groupMap.IdGroup);

            stmt = sb.GetStatement();

            RadioChannelGroup channelGroup = ObjectFactory.GetInstance<RadioChannelGroup>(stmt.Execute());

            groupNames.Add(channelGroup.GroupName);
          }
        }

        return groupNames;
      }
    }

    #endregion

    #region Storage and Retrieval

    /// <summary>
    /// Static method to retrieve all instances that are stored in the database in one call
    /// </summary>
    public static IList<Channel> ListAll()
    {
      return Broker.RetrieveList<Channel>();
    }

    /// <summary>
    /// Retrieves an entity given it's id.
    /// </summary>
    public static Channel Retrieve(int id)
    {
      // Return null if id is smaller than seed and/or increment for autokey
      if (id < 1)
      {
        return null;
      }
      Key key = new Key(typeof (Channel), true, "idChannel", id);
      return Broker.TryRetrieveInstance<Channel>(key);
    }

    /// <summary>
    /// Retrieves an entity given it's id, using Gentle.Framework.Key class.
    /// This allows retrieval based on multi-column keys.
    /// </summary>
    public static Channel Retrieve(Key key)
    {
      return Broker.RetrieveInstance<Channel>(key);
    }

    /// <summary>
    /// Persists the entity if it was never persisted or was changed.
    /// </summary>
    public override void Persist()
    {
      if (IsChanged || !IsPersisted)
      {
        try
        {
          base.Persist();
        }
        catch (Exception ex)
        {
          Log.Error("Exception in Channel.Persist() with Message {0}", ex.Message);
          Log.Write(ex);
          return;
        }
        isChanged = false;
      }
    }

    #endregion

    #region Relations

    /// <summary>
    /// Get a list of ChannelMap referring to the current entity.
    /// </summary>
    public IList<ChannelMap> ReferringChannelMap()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (ChannelMap));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<ChannelMap>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(ChannelMap), this );
    }

    /// <summary>
    /// Get a list of GroupMap referring to the current entity.
    /// </summary>
    public IList<GroupMap> ReferringGroupMap()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (GroupMap));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<GroupMap>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(GroupMap), this );
    }

    /// <summary>
    /// Get a list of RadioGroupMap referring to the current entity.
    /// </summary>
    public IList<RadioGroupMap> ReferringRadioGroupMap()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (RadioGroupMap));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<RadioGroupMap>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(GroupMap), this );
    }

    /// <summary>
    /// Get a list of Conflicts referring to the current entity.
    /// </summary>
    public IList<Conflict> ReferringConflicts()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Conflict));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<Conflict>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(GroupMap), this );
    }

    /// <summary>
    /// Get a list of Program referring to the current entity.
    /// </summary>
    public IList<Program> ReferringProgram()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<Program>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(Program), this );
    }

    /// <summary>
    /// Get a list of Recording referring to the current entity.
    /// </summary>
    public IList<Recording> ReferringRecording()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Recording));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<Recording>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(Recording), this );
    }

    /// <summary>
    /// Get a list of Schedule referring to the current entity.
    /// </summary>
    public IList<Schedule> ReferringSchedule()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Schedule));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<Schedule>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(Schedule), this );
    }

    /// <summary>
    /// Get a list of TuningDetail referring to the current entity.
    /// </summary>
    public IList<TuningDetail> ReferringTuningDetail()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (TuningDetail));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(TuningDetail), this );
    }

    /// <summary>
    /// Get a list of TuningDetail referring to the current entity.
    /// </summary>
    public IList<History> ReferringHistory()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (History));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<History>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(TuningDetail), this );
    }

    /// <summary>
    /// Get a list of linked channels referring to the current entity.
    /// </summary>
    public IList<ChannelLinkageMap> ReferringLinkedChannels()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (ChannelLinkageMap));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idPortalChannel", idChannel);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection<ChannelLinkageMap>(stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(ChannelLinkageMap), this );
    }

    #endregion

    public Program NextProgram
    {
      get
      {
        UpdateNowAndNext();
        return _nextProgram;
      }
    }

    public Program CurrentProgram
    {
      get
      {
        UpdateNowAndNext();
        return _currentProgram;
      }
    }

    public Program GetProgramAt(DateTime date)
    {
      //IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      //DateTime startTime = DateTime.Now;
       SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
       sb.AddConstraint(Operator.Equals, "idChannel", IdChannel);
       sb.AddConstraint(Operator.GreaterThan, "endTime", date);
       sb.AddConstraint(Operator.LessThanOrEquals, "startTime", date);
       sb.AddOrderByField(true, "startTime");
       sb.SetRowLimit(1);
       SqlStatement stmt = sb.GetStatement(true);
       IList<Program> programs = ObjectFactory.GetCollection<Program>(stmt.Execute());
       if (programs.Count == 0)
       {
         return null;
       }
       return programs[0];
     }
 
     public Program GetProgramAt(DateTime date, int tt, string seriesId)
     {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));
      sb.AddConstraint(Operator.Equals, "idChannel", IdChannel);
      sb.AddConstraint(Operator.Equals, "seriesId", seriesId);
      sb.AddConstraint(Operator.GreaterThan, "endTime", date);
      sb.AddConstraint(Operator.LessThanOrEquals, "startTime", date);
      sb.AddOrderByField(true, "startTime");
      sb.SetRowLimit(1);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Program> programs = ObjectFactory.GetCollection<Program>(stmt.Execute());
      if (programs.Count == 0)
      {
        return null;
      }
      return programs[0];
    }

     /// <summary>
     /// Added for Plugins to remove a channel quickly from all groups.
     /// </summary>
     public void RemoveFromAllGroups()
     {
         if (IsRadio)
         {
             Broker.Execute("delete from RadioGroupMap WHERE idChannel=" + idChannel);
         }
         else
         {
             Broker.Execute("delete from GroupMap WHERE idChannel=" + idChannel);
         }
     }

    public Program GetProgramAt(DateTime date, string title)
    {
      //IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      //DateTime startTime = DateTime.Now;
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));
      sb.AddConstraint(Operator.Equals, "Title", title);
      sb.AddConstraint(Operator.Equals, "idChannel", IdChannel);
      sb.AddConstraint(Operator.GreaterThan, "endTime", date);
      sb.AddConstraint(Operator.LessThanOrEquals, "startTime", date);
      sb.AddOrderByField(true, "startTime");
      sb.SetRowLimit(1);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Program> programs = ObjectFactory.GetCollection<Program>(stmt.Execute());
      if (programs.Count == 0)
      {
        return null;
      }
      return programs[0];
    }

    private void UpdateNowAndNext()
    {
      if (_currentProgram != null)
      {
        if (DateTime.Now >= _currentProgram.StartTime && DateTime.Now <= _currentProgram.EndTime)
        {
          return;
        }
      }

      _currentProgram = null;
      _nextProgram = null;

      DateTime date = DateTime.Now;
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));
      sb.AddConstraint(Operator.Equals, "idChannel", IdChannel);
      sb.AddConstraint(Operator.GreaterThanOrEquals, "endTime", date);
      sb.AddOrderByField(true, "startTime");
      sb.SetRowLimit(2);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Program> programs = ObjectFactory.GetCollection<Program>(stmt.Execute());
      if (programs.Count == 0)
      {
        return;
      }
      _currentProgram = programs[0];
      if (_currentProgram.StartTime >= date)
      {
        _nextProgram = _currentProgram;
        _currentProgram = null;
      }
      else
      {
        if (programs.Count == 2)
        {
          _nextProgram = programs[1];
        }
      }
    }

    public void Delete()
    {
      Broker.Execute("delete from History WHERE idChannel=" + idChannel);
      Broker.Execute("delete from Conflict WHERE idChannel=" + idChannel);
      Broker.Execute("delete from Program WHERE idChannel=" + idChannel);
      Broker.Execute("delete from Schedule WHERE idChannel=" + idChannel);
      // recordings should be stay in the DB
      //Gentle.Framework.Broker.Execute("delete from Recording WHERE idChannel=" + idChannel.ToString());
      Broker.Execute("delete from ChannelMap WHERE idChannel=" + idChannel);
      Broker.Execute("delete from TuningDetail WHERE idChannel=" + idChannel);

      if (IsRadio)
      {
        Broker.Execute("delete from RadioGroupMap WHERE idChannel=" + idChannel);
      }
      else
      {
        Broker.Execute("delete from GroupMap WHERE idChannel=" + idChannel);
        Broker.Execute("delete from ChannelLinkageMap WHERE idPortalChannel=" + idChannel + " OR idLinkedChannel=" +
                       idChannel);
      }
      Remove();
    }

    public bool ContainsChannelType(int channelType)
    {
      foreach (TuningDetail detail in ReferringTuningDetail())
      {
        if (detail.ChannelType == channelType)
        {
          return true;
        }
      }
      return false;
    }

    public bool IsWebstream()
    {
      IList<TuningDetail> details = ReferringTuningDetail();
      if (details == null)
      {
        return false;
      }
      foreach (TuningDetail detail in details)
      {
        if (detail.ChannelType == 5)
        {
          return true;
        }
      }
      return false;
    }
  }
}