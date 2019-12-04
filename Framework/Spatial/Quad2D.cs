﻿using System;

namespace Foster.Framework
{
    public struct Quad2D : IConvexShape2D
    {

        private Vector2 a;
        private Vector2 b;
        private Vector2 c;
        private Vector2 d;
        private Vector2 normalAB;
        private Vector2 normalBC;
        private Vector2 normalCD;
        private Vector2 normalDA;
        private bool dirty;

        public Vector2 A
        {
            get => a;
            set
            {
                if (a != value)
                {
                    a = value;
                    dirty = true;
                }
            }
        }

        public Vector2 B
        {
            get => b;
            set
            {
                if (b != value)
                {
                    b = value;
                    dirty = true;
                }
            }
        }

        public Vector2 C
        {
            get => c;
            set
            {
                if (c != value)
                {
                    c = value;
                    dirty = true;
                }
            }
        }

        public Vector2 D
        {
            get => d;
            set
            {
                if (d != value)
                {
                    d = value;
                    dirty = true;
                }
            }
        }

        public Vector2 NormalAB
        {
            get
            {
                if (dirty)
                    UpdateQuad();
                return normalAB;
            }
        }

        public Vector2 NormalBC
        {
            get
            {
                if (dirty)
                    UpdateQuad();
                return normalBC;
            }
        }

        public Vector2 NormalCD
        {
            get
            {
                if (dirty)
                    UpdateQuad();
                return normalCD;
            }
        }

        public Vector2 NormalDA
        {
            get
            {
                if (dirty)
                    UpdateQuad();
                return normalDA;
            }
        }

        public Vector2 Center => (a + b + c + d) / 4f;


        public Quad2D(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            normalAB = normalBC = normalCD = normalDA = Vector2.Zero;
            dirty = true;
        }

        private void UpdateQuad()
        {
            normalAB = (b - a).Normalized.TurnLeft;
            normalBC = (c - b).Normalized.TurnLeft;
            normalCD = (d - c).Normalized.TurnLeft;
            normalDA = (a - d).Normalized.TurnLeft;

            dirty = false;
        }

        public Quad2D Translate(Vector2 amount)
        {
            A += amount;
            B += amount;
            C += amount;
            D += amount;
            return this;
        }

        public void Project(Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            var dot = Vector2.Dot(A, axis);
            min = Math.Min(dot, min);
            max = Math.Max(dot, max);
            dot = Vector2.Dot(B, axis);
            min = Math.Min(dot, min);
            max = Math.Max(dot, max);
            dot = Vector2.Dot(C, axis);
            min = Math.Min(dot, min);
            max = Math.Max(dot, max);
            dot = Vector2.Dot(D, axis);
            min = Math.Min(dot, min);
            max = Math.Max(dot, max);
        }

        public int Sides => 4;

        public Vector2 GetPoint(int index)
        {
            return index switch
            {
                0 => A,
                1 => B,
                2 => C,
                3 => D,
                _ => throw new IndexOutOfRangeException(),
            };
        }

        public Rect BoundingRect()
        {
            var bounds = new Rect();
            bounds.X = Math.Min(a.X, Math.Min(b.X, Math.Min(c.X, d.X)));
            bounds.Y = Math.Min(a.Y, Math.Min(b.Y, Math.Min(c.Y, d.Y)));
            bounds.Width = Math.Max(a.X, Math.Max(b.X, Math.Max(c.X, d.X))) - bounds.X;
            bounds.Height = Math.Max(a.Y, Math.Max(b.Y, Math.Max(c.Y, d.Y))) - bounds.Y;
            return bounds;
        }

        public override bool Equals(object? obj) => (obj is Quad2D other) && (this == other);

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + A.GetHashCode();
            hash = hash * 23 + B.GetHashCode();
            hash = hash * 23 + C.GetHashCode();
            hash = hash * 23 + D.GetHashCode();
            return hash;
        }

        public static Quad2D Transform(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Matrix2D matrix)
        {
            return new Quad2D(
                Vector2.Transform(a, matrix),
                Vector2.Transform(b, matrix),
                Vector2.Transform(c, matrix),
                Vector2.Transform(d, matrix));
        }

        public static Quad2D Transform(Quad2D quad, Matrix2D matrix)
        {
            return new Quad2D(
                Vector2.Transform(quad.a, matrix),
                Vector2.Transform(quad.b, matrix),
                Vector2.Transform(quad.c, matrix),
                Vector2.Transform(quad.d, matrix));
        }

        public static bool operator ==(Quad2D a, Quad2D b)
        {
            return a.A == b.A && a.B == b.B && a.C == b.C && a.D == b.D;
        }

        public static bool operator !=(Quad2D a, Quad2D b)
        {
            return a.A != b.A || a.B != b.B || a.C != b.C || a.D != b.D;
        }
    }
}
