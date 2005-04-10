using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using SQLite.NET;
using MediaPortal.GUI.View;
using MediaPortal.GUI.Library;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for VideoViewHandler.
	/// </summary>
	public class VideoViewHandler
	{


		ViewDefinition currentView;
		int						 currentLevel=0;
		ArrayList      views=new ArrayList();					
		public VideoViewHandler()
		{
			using(FileStream fileStream = new FileStream("videoViews.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				try
				{
					SoapFormatter formatter = new SoapFormatter();
					views = (ArrayList)formatter.Deserialize(fileStream);
					fileStream.Close();
				}
				catch
				{
				}
			}
		}

		public ViewDefinition View
		{
			get { return currentView; }
			set { currentView=value;}
		}


		public ArrayList Views
		{
			get { return views; }
			set { views=value;}
		}

		public string CurrentView 
		{
			get 
			{ 
				if (currentView == null) 
					return String.Empty;
				return currentView.Name; 
			}
			set 
			{ 
				foreach (ViewDefinition definition in views)
				{
					if (definition.Name == value) 
					{
						View=definition;
						CurrentLevel=0;
					}
				}
			}
		}

		public int CurrentLevel
		{
			get { return currentLevel;}
			set 
			{ 
				if (value < 0 || value >= currentView.Filters.Count) return;
				currentLevel=value;
			}
		}
		public int MaxLevels
		{
			get { return currentView.Filters.Count;}
		}
		
		public void Select(IMDBMovie movie)
		{
			FilterDefinition definition=(FilterDefinition)currentView.Filters[CurrentLevel];
			definition.SelectedValue=GetFieldIdValue(movie,definition.Where).ToString();
			if (currentLevel+1 < currentView.Filters.Count) currentLevel++;

		}
		public ArrayList Execute()
		{
			//build the query
			ArrayList movies=new ArrayList();
			string whereClause=String.Empty;
			string orderClause=String.Empty;
			if (CurrentLevel >0)
			{
				whereClause="where movieinfo.idmovie=movie.idmovie and movieinfo.iddirector=actors.idActor and movie.idpath=path.idpath";
			}

			for (int i=0; i < CurrentLevel;++i)
			{
				BuildSelect((FilterDefinition)currentView.Filters[i],ref whereClause );
			}
			BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel],ref whereClause );
			BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel],ref whereClause );
			BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel],ref orderClause );

			//execute the query
			string sql;
			if (CurrentLevel==0)
			{
				bool useMovieInfoTable=false;
				bool useAlbumTable=false;
				bool useActorsTable=false;
				bool useGenreTable=false;
				FilterDefinition defRoot=(FilterDefinition)currentView.Filters[0];
				string table=GetTable(defRoot.Where,ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,ref useGenreTable);

				if (table=="actor")
				{
					sql=String.Format("select * from actors ");
					if (whereClause!=String.Empty) sql+= "where "+whereClause;
					if (orderClause!=String.Empty) sql+= orderClause;
					VideoDatabase.GetMoviesByFilter(sql, out movies,true, false, false);
				}
				else if (table=="genre")
				{
					sql=String.Format("select * from genre ");
					if (whereClause!=String.Empty) sql+= "where "+whereClause;
					if (orderClause!=String.Empty) sql+= orderClause;
					VideoDatabase.GetMoviesByFilter(sql, out movies,false, false, true);
				}
				else if (defRoot.Where=="year")
				{
					movies = new ArrayList();
					sql=String.Format("select distinct iYear from movieinfo ");
					SQLiteResultSet results=VideoDatabase.GetResults(sql);
					for (int i=0; i<results.Rows.Count; i++)
					{
						IMDBMovie movie = new IMDBMovie();
						movie.Year =  (int)Math.Floor(0.5d+Double.Parse(Database.DatabaseUtility.Get(results,i,"iYear")));
						movies.Add(movie);
					}
				}
				else
				{
					whereClause="where movieinfo.idmovie=movie.idmovie and movieinfo.iddirector=actors.idActor and movie.idpath=path.idpath";
					BuildRestriction(defRoot,ref whereClause );
					sql=String.Format("select * from movie,movieinfo,actors,path  {0} {1}",
						whereClause,orderClause);
					VideoDatabase.GetMoviesByFilter(sql, out movies,true, true, true);
				}
			}
			else if (CurrentLevel < MaxLevels-1)
			{
				bool useMovieInfoTable=false;
				bool useAlbumTable=false;
				bool useActorsTable=false;
				bool useGenreTable=false;
				FilterDefinition defCurrent=(FilterDefinition)currentView.Filters[CurrentLevel];
				string table=GetTable(defCurrent.Where,ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,ref useGenreTable);
				sql=String.Format("select distinct {0}.* from movie,movieinfo,actors,path {1} {2}",
					table,whereClause,orderClause);
				VideoDatabase.GetMoviesByFilter(sql, out movies,useActorsTable, useMovieInfoTable, useGenreTable);
				
			}
			else
			{
				sql=String.Format("select * from movie,movieinfo,actors,path {0} {1}",
					whereClause,orderClause);
				VideoDatabase.GetMoviesByFilter(sql, out movies,true, true, true);
			}
			return movies;
		}

		void BuildSelect(FilterDefinition filter,ref string whereClause )
		{
			if (whereClause!="") whereClause+=" and ";
			whereClause+=String.Format(" {0}='{1}'",GetFieldId(filter.Where),filter.SelectedValue);
		}
		void BuildRestriction(FilterDefinition filter,ref string whereClause )
		{
			if (filter.SqlOperator != String.Empty && filter.Restriction != String.Empty)
			{
				if (whereClause!="") whereClause+=" and ";
				string restriction=filter.Restriction;
				restriction=restriction.Replace("*","%");
				Database.DatabaseUtility.RemoveInvalidChars(ref restriction);
				if (filter.SqlOperator=="=")
				{
					bool isascii=false;
					for (int x=0; x < restriction.Length;++x)
					{
						if ( !Char.IsDigit(restriction[x]) )
						{
							isascii=true;
							break;
						}
					}
					if (isascii)
					{
						filter.SqlOperator="like";
					}
				}
				whereClause+=String.Format(" {0} {1} '{2}'",GetFieldName(filter.Where),filter.SqlOperator,restriction);
			}
		}
		void BuildWhere(FilterDefinition filter,ref string whereClause )
		{
			if (filter.WhereValue!="*") 
			{
				if (whereClause!="") whereClause+=" and ";
				whereClause+=String.Format(" {0}='{1}'",GetField(filter.Where),filter.WhereValue);
			}
		}

		void BuildOrder(FilterDefinition filter,ref string orderClause )
		{
			orderClause=" order by "+GetField(filter.Where) + " ";
			if (!filter.SortAscending) orderClause+="desc";
			else orderClause+="asc";
			if (filter.Limit>0)
			{
				orderClause += String.Format(" Limit {0}", filter.Limit);
			}
		}
		string GetTable(string where,ref bool useMovieInfoTable, ref bool useAlbumTable, ref bool useActorsTable,ref bool useGenreTable)
		{
			if (where=="actor") { useActorsTable=true;return "actors";}
			if (where=="title") { useMovieInfoTable=true;return "movieinfo";}
			if (where=="genre") { useGenreTable=true;return "genre";}
			if (where=="year") { useMovieInfoTable=true;return "movieinfo";}
			if (where=="rating") { useMovieInfoTable=true;return "movieinfo";}
			return null;
		}
		string GetField(string where)
		{
			if (where=="actor") return "strActor";
			if (where=="title") return "strTitle";
			if (where=="genre") return "strGenre";
			if (where=="year") return "iYear";
			if (where=="rating") return "fRating";
			return null;
		}
		string GetFieldId(string where)
		{
			if (where=="actor") return "actors.idActor";
			if (where=="title") return "movieinfo.idMovie";
			if (where=="genre") return "genre.idGenre";
			if (where=="year") return "movieinfo.iYear";
			if (where=="rating") return "movieinfo.fRating";
			return null;
		}
		string GetFieldName(string where)
		{
			if (where=="actor") return "actor.strActor";
			if (where=="title") return "movieinfo.strTitle";
			if (where=="genre") return "genre.strGenre";
			if (where=="year") return "movieinfo.iYear";
			if (where=="rating") return "movieinfo.fRating";
			return null;
		}
		int GetFieldIdValue(IMDBMovie movie,string where)
		{
			if (where=="actor")  return movie.actorId;
			if (where=="title")  return movie.ID;
			if (where=="genre")  return movie.genreId;
			if (where=="year")   return movie.Year;
			if (where=="rating") return (int)movie.Rating;
			return -1;
		}

		public void SetLabel(IMDBMovie movie,ref GUIListItem item)
		{
			if (movie==null) return;
			FilterDefinition definition=(FilterDefinition)currentView.Filters[CurrentLevel];
			if (definition.Where=="genre")
			{
				item.Label=movie.Genre;
				item.Label2=String.Empty;
				item.Label3=String.Empty;
			}
			if (definition.Where=="actor")
			{
				item.Label=movie.Actor;
				item.Label2=String.Empty;
				item.Label3=String.Empty;
			}
			if (definition.Where=="year")
			{
				item.Label=movie.Year.ToString();
				item.Label2=String.Empty;
				item.Label3=String.Empty;
			}

		}
	}
}
