using static System.Net.Mime.MediaTypeNames;

namespace LongTrip
{
    internal class Program
    {
        static Stack<int> save = new Stack<int>();
        static float maxDistance = 0;


        static void Main(string[] args)
        {

            while (true)
            {
                var line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }

                var infos = line.Split(",");
                int startID = int.Parse(infos[0]);
                int nextID = int.Parse(infos[1]);
                float dis = float.Parse(infos[2]);

                if (!StationList.Instance.Contains(startID))
                {
                    StationList.Instance.AddStation(new Station(startID));
                }

                if (!StationList.Instance.Contains(nextID))
                {
                    StationList.Instance.AddStation(new Station(nextID));
                }

                StationList.Instance.GetStation(startID).AddNextStation(new NextStation(startID, nextID, dis));
            }

            Stack<int> path = new Stack<int>();
            float distance;
            
            foreach(Station station in StationList.Instance.Stations)
            {
                path.Push(station.ID);
                distance = 0;

                while (path.Count > 0)
                {
                    Station s = StationList.Instance.GetStation(path.Peek());

                    if (s.ID == path.Last() && path.Count > 1) //Startの駅と同じか
                    {
                        if (maxDistance < distance)
                        {
                            maxDistance = distance;
                            save = new Stack<int>(path);
                        }

                        int id = path.Pop();

                        if (path.TryPeek(out int result))
                        {
                            distance -= StationList.Instance.GetStation(result).GetNextStation(id).Distance;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (s.TryGetNextStation(path, out NextStation next)) //次に移動できる駅があるか
                    {
                        path.Push(next.NextID);
                        StationList.Instance.GetStation(next.NextID).AddUsedPath(new UsedPath(path));
                        distance += next.Distance;
                    }
                    else //移動できないとき
                    {
                        if(maxDistance < distance)
                        {
                            maxDistance = distance;
                            save = new Stack<int>(path);
                        }

                        int id = path.Pop();

                        if (path.TryPeek(out int result))
                        {
                            distance -= StationList.Instance.GetStation(result).GetNextStation(id).Distance;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                foreach (Station s in StationList.Instance.Stations)
                {
                    s.ClearUsedPath();
                }
            }

            //Console.WriteLine(maxDistance);

            foreach(int id in save)
            {
                Console.WriteLine(id);
            }
        }
    }

    internal class Station
    {
        private int _id;
        private List<UsedPath> _usedPaths;
        private List<NextStation> _nextStation;

        public Station(int id)
        {
            _id = id;
            _usedPaths = new List<UsedPath>();
            _nextStation = new List<NextStation>();
        }

        public int ID => _id;
        public List<NextStation> NextStations => _nextStation;


        public void AddUsedPath(UsedPath usedPath)
        {
            if (HasUsedPath(usedPath))
            {
                return;
            }

            _usedPaths.Add(usedPath);
        }

        public bool HasUsedPath(UsedPath usedPath)
        {
            foreach (UsedPath p in _usedPaths)
            {
                if (p.Equals(usedPath))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasUsedPath(Stack<int> usedPath) 
        {
            foreach (UsedPath p in _usedPaths)
            {
                if (p.Path.SequenceEqual(usedPath))
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearUsedPath()
        {
            _usedPaths.Clear();
        }

        public void AddNextStation(NextStation nextStation)
        {
            if (_id != nextStation.StartID)
            {
                return;
            }

            _nextStation.Add(nextStation);
        }

        public NextStation GetNextStation(int nextID)
        {
            foreach(NextStation nextStation in _nextStation)
            {
                if (nextStation.NextID == nextID)
                {
                    return nextStation;
                }
            }

            return null;
        }

        public bool TryGetNextStation(UsedPath usedPath, out NextStation next)
        {
            next = null;

            foreach (NextStation nextStation in _nextStation)
            {
                if (StationList.Instance.GetStation(nextStation.NextID).HasUsedPath(usedPath))
                {
                    continue;
                }

                if (usedPath.Path.Contains(nextStation.NextID))
                {
                    if (usedPath.Path.Last() != nextStation.NextID)
                    {
                        continue;
                    }
                }

                next = nextStation;
                return true;
            }

            return false;
        }

        public bool TryGetNextStation(Stack<int> usedPath, out NextStation next)
        {
            next = null;

            foreach (NextStation nextStation in _nextStation)
            {
                if (StationList.Instance.GetStation(nextStation.NextID).HasUsedPath(usedPath))
                {
                    continue;
                }

                if (usedPath.Contains(nextStation.NextID))
                {
                    if (usedPath.Last() != nextStation.NextID)
                    {
                        continue;
                    }
                }

                next = nextStation;
                return true;
            }

            return false;
        }

    }

    internal class UsedPath
    {
        private Stack<int> _usedPath;

        public UsedPath(Stack<int> usedPath)
        {
            _usedPath = usedPath;
        }

        public Stack<int> Path => _usedPath;

        public bool Equals(UsedPath usedPath)
        {
            return _usedPath.SequenceEqual(usedPath.Path);
        }
    }

    internal class NextStation
    {
        private int _startID;
        private int _nextID;
        private float _distance;

        public NextStation(int startID, int endID, float distance)
        {
            _startID = startID;
            _nextID = endID;
            _distance = distance;
        }

        public int StartID => _startID;
        public int NextID => _nextID;
        public float Distance => _distance;

    }

    internal class StationList
    {
        private List<Station> _stations;

        private static StationList _instance = null;

        private StationList()
        {
            _stations = new List<Station>();
        }

        public List<Station> Stations
        {
            get
            {
                List<Station> list = new List<Station>(_stations);
                return list;
            }
        }

        public static StationList Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new StationList();
                }
                
                return _instance;
            }
        }

        public void AddStation(Station station)
        {
            if (Contains(station))
            {
                return;
            }

            _stations.Add(station);
        }

        public bool Contains(Station station)
        {
            foreach (Station s in _stations)
            {
                if (s.ID == station.ID)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(int id)
        {
            foreach (Station s in _stations)
            {
                if (s.ID == id)
                {
                    return true;
                }
            }

            return false;
        }

        public Station GetStation(int id)
        {
            foreach(Station s in _stations)
            {
                if(s.ID == id)
                {
                    return s;
                }
            }

            return null;
        }
    }
}
