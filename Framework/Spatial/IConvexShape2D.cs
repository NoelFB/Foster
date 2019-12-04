﻿using System;

namespace Foster.Framework
{
    public interface IConvexShape2D : IProjectable2D
    {
        public int Sides { get; }
        public Vector2 GetPoint(int index);
    }

    public static class IConvexShape2DExt
    {
        public static bool Overlaps(this IConvexShape2D a, Circle b, out Vector2 pushout)
        {
            pushout = Vector2.Zero;

            var distance = float.MaxValue;

            // check against axis
            var last = a.GetPoint(a.Sides - 1);
            for (int i = 0; i < a.Sides; i++)
            {
                var next = a.GetPoint(i);
                var axis = (next - last).Normalized;

                if (!a.AxisOverlaps(b, axis, out float amount))
                    return false;

                if (Math.Abs(amount) < distance)
                {
                    pushout = axis * amount;
                    distance = Math.Abs(amount);
                }

                last = next;
            }

            // check against vertices
            for (int i = 0; i < a.Sides; i++)
            {
                var axis = (a.GetPoint(i) - b.Position).Normalized;

                if (!a.AxisOverlaps(b, axis, out float amount))
                    return false;

                if (Math.Abs(amount) < distance)
                {
                    pushout = axis * amount;
                    distance = Math.Abs(amount);
                }
            }

            return true;
        }

        public static bool Overlaps(this IConvexShape2D a, IConvexShape2D b, out Vector2 pushout)
        {
            pushout = Vector2.Zero;

            var distance = float.MaxValue;

            // a-axis
            {
                var last = a.GetPoint(a.Sides - 1);
                for (int i = 0; i < a.Sides; i++)
                {
                    var next = a.GetPoint(i);
                    var axis = (next - last).Normalized;

                    if (!a.AxisOverlaps(b, axis, out float amount))
                        return false;

                    if (Math.Abs(amount) < distance)
                    {
                        pushout = axis * amount;
                        distance = Math.Abs(amount);
                    }

                    last = next;
                }
            }

            // b-axis
            {
                var last = b.GetPoint(b.Sides - 1);
                for (int i = 0; i < b.Sides; i++)
                {
                    var next = b.GetPoint(i);
                    var axis = (next - last).Normalized;

                    if (!b.AxisOverlaps(a, axis, out float amount))
                        return false;

                    if (Math.Abs(amount) < distance)
                    {
                        pushout = axis * amount;
                        distance = Math.Abs(amount);
                    }

                    last = next;
                }
            }

            return true;
        }
    }
}
