using System;
using System.Collections.Generic;
using Godot;
namespace Algos
{
    public enum TransitionState { FAILURE, SUCCESS, TRANSITIONING };
    public enum TargetTypes { SELF, ALLY, ENEMY, OBJECT, EMPTY, UNKNOWN }
    public enum TargetCoditioning { ALIVE, DEAD, POISONED, BURNING, CONFUSED }

    //TODO: Implement more shapes: Line, Triangle, T?
    public enum Shape { Circle, Square }

    //* TODO: Repensar o nome desta class. Talvez GameBoardUtils? Ou passar isto tudo para a Grid / GameBoard?
    public static class FloodFill
    {

        public static int CalculateDistanceBetweenTwoCells(Vector2 cell1, Vector2 cell2)
        {
            int distance = (int)(Mathf.Abs(cell1.X - cell2.X) + Mathf.Abs(cell1.Y - cell2.Y));
            return distance;
        }
        public static bool HasReachedMaxDistanceCircle(Vector2 current, Vector2 startingCell, int maxDistance)
        {
            Vector2 difference = (current - startingCell).Abs();
            int distance = (int)(difference.X + difference.Y);
            return distance > maxDistance;
        }
        public static bool HasReachedMaxDistanceSquare(Vector2 current, Vector2 startingCell, int maxDistance)
        {
            int distanceX = Math.Abs((int)current.X - (int)startingCell.X);
            int distanceY = Math.Abs((int)current.Y - (int)startingCell.Y);
            // Ensure that both X and Y distances are within the maxDistance limit
            return !(distanceX < maxDistance && distanceY < maxDistance);
        }

        public static bool HasBeenVisited(List<Vector2> cellsToCheck, Vector2 cell)
        {
            return cellsToCheck.Contains(cell);
        }
    }

    public static class TimerUtils
    {

        public static void CreateTimer(Action action, Node node, float waitTime)
        {
            if (node.GetTree().IsQueuedForDeletion()) return;
            Timer timer = new()
            {
                WaitTime = waitTime
            };
            timer.AddToGroup("Timers");
            timer.Timeout += () => OnTimerTimeout(timer, action, node);
            node.AddChild(timer);
            timer.Start();
        }

        public static void OnTimerTimeout(Timer timer, Action action, Node node)
        {
            if (node.IsQueuedForDeletion()) return;
            action();
            timer.Stop();
            node.RemoveChild(timer);
            timer.QueueFree();
        }
    }

    public static class NodeUtils
    {
        public static Node GetInstantiatedScene(string scenePath)
        {
            PackedScene scene = GD.Load<PackedScene>(scenePath);
            return scene.Instantiate();
        }
    }
}
