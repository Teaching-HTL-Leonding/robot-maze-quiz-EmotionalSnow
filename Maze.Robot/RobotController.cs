using System.Collections.Generic;
using Maze.Library;

namespace Maze.Solver
{
    /// <summary>
    /// Moves a robot from its current position towards the exit of the maze
    /// </summary>
    public class RobotController
    {
        private readonly struct Coordinates
        {
            private readonly int _x;
            private readonly int _y;

            public Coordinates(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public Coordinates GetNeighbour(Direction direction)
            {
                var directionValue = (int) direction;
                var x = _x + (directionValue < 2 ? decimal.Compare(0.5m, directionValue) : 0);
                var y = _y + (directionValue < 2 ? 0 : decimal.Compare(2.5m, directionValue));
                
                return new Coordinates(x, y);
            }
        }
        
        private readonly IRobot robot;
        private readonly Dictionary<Coordinates, bool[]?> map = new Dictionary<Coordinates, bool[]?>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotController"/> class
        /// </summary>
        /// <param name="robot">Robot that is controlled</param>
        public RobotController(IRobot robot)
        {
            this.robot = robot;
        }

        /// <summary>
        /// Moves the robot to the exit
        /// </summary>
        /// <remarks>
        /// This function uses methods of the robot that was passed into this class'
        /// constructor. It has to move the robot until the robot's event
        /// <see cref="IRobot.ReachedExit"/> is fired. If the algorithm finds out that
        /// the exit is not reachable, it has to call <see cref="IRobot.HaltAndCatchFire"/>
        /// and exit.
        /// </remarks>
        public void MoveRobotToExit()
        {
            var reachedEnd = false;
            robot.ReachedExit += (_, __) => reachedEnd = true;

            bool[] GetPossibleMoveDirections(Coordinates coordinates)
            {
                var neighbours = new bool[4];
                for (var x = 0; x < 4; x++)
                {
                    var direction = (Direction) x;
                    var canMove = robot.CanIMove(direction);
                    
                    neighbours[x] = canMove;
                    
                    if (canMove) map.TryAdd(coordinates.GetNeighbour(direction), null);
                }

                return neighbours;
            }
            
            void MakeNextMove(Coordinates coordinates)
            {
                if (reachedEnd) return;
                
                static Direction InvertDirection(Direction direction) => (Direction) ((((int) direction + 1) % 2) + (2 * ((int) direction / 2)));
                
                var neighbours = GetPossibleMoveDirections(coordinates);
                map[coordinates] = neighbours;

                for (var x = 0; x < 4; x++)
                {
                    if (!neighbours[x]) continue;

                    var direction = (Direction) x;
                    var neighbour = coordinates.GetNeighbour(direction);
                    
                    if (map[neighbour] != null) continue;
                    
                    robot.Move(direction);
                    MakeNextMove(neighbour);
                    
                    if (!reachedEnd) robot.Move(InvertDirection(direction));
                    else return;
                }
            }
            
            MakeNextMove(new Coordinates(0, 0));
            
            if (!reachedEnd) robot.HaltAndCatchFire();
        }
    }
}
