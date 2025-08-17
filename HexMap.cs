using System;
using System.Collections.Generic;
using UnityEngine;


public class HexMap : MonoBehaviour
{
    public Dictionary<HexCoord, HexTile> Tiles = new();
    public float Size = 1f;
    
    // ==== Section : Conversion ==== \\
    public Vector3 HexToWorld(HexCoord hex)
    {
        float x = Size * (Mathf.Sqrt(3f) * hex.Q + Mathf.Sqrt(3f) / 2f * hex.R);
        float z = Size * (1.5f * hex.R);
        return new Vector3(x, 0, z);
    }

    public HexCoord WorldToHex(Vector3 position)
    {
        float qf = (Mathf.Sqrt(3f) / 3f * position.x - 1f / 3f * position.z) / Size;
        float rf = (2f / 3f * position.z) / Size;

        return CubeRound(qf, rf, -qf - rf);
    }

    // ==== Section : Utility ==== \\
    private HexCoord CubeRound(float qf, float rf, float sf)
    {
        int q = Mathf.RoundToInt(qf);
        int r = Mathf.RoundToInt(rf);
        int s = Mathf.RoundToInt(sf);

        float dq = Mathf.Abs(q - qf);
        float dr = Mathf.Abs(r - rf);
        float ds = Mathf.Abs(s - sf);

        if (dq > dr && dq > ds)
            q = -r - s;
        else if (dr > ds)
            r = -q - s;

        return new HexCoord(q, r);
    }
}


public readonly struct HexCoord : IEquatable<HexCoord>
{
    // ==== Section : Properties ==== \\
    public readonly int Q, R;
    public int S() => -Q - R;
    
    // ==== Section : Constructors ==== \\
    public HexCoord(int q, int r)
    {
        Q = q;
        R = r;
    }
    
    public static HexCoord zero = new(0, 0);
    
    public static HexCoord one = new(1, 1);
    
    // ==== Section : Math ==== \\
    public static HexCoord operator +(HexCoord a, HexCoord b) => new(a.Q + b.Q, a.R + b.R);
    public static HexCoord operator -(HexCoord a, HexCoord b) => new(a.Q - b.Q, a.R - b.R);
    public static HexCoord operator *(HexCoord a, int b) => new(a.Q * b, a.R * b);

    public static int Distance(HexCoord a, HexCoord b)
    {
        return (Math.Abs(a.Q - b.Q)
                + Math.Abs(a.R - b.R)
                + Math.Abs(a.S() - b.S())) / 2;
    }
    
    // ==== Section : Shapes ==== \\
    private static readonly HexCoord[] Directions = new HexCoord[]
    {
        new HexCoord(1, 0),
        new HexCoord(1, -1),
        new HexCoord(0, -1),
        new HexCoord(-1, 0),
        new HexCoord(-1, 1),
        new HexCoord(0, 1),
    };

    public static IEnumerable<HexCoord> Ring(HexCoord center, int radius)
    {
        if (radius == 0)
        {
            yield return center;
            yield break;
        }

        // Start: Richtung 4 * radius Schritte
        var hex = center + Directions[4] * radius;

        for (int side = 0; side < 6; side++)
        {
            for (int step = 0; step < radius; step++)
            {
                yield return hex;
                hex += Directions[side];
            }
        }
    }

    public static IEnumerable<HexCoord> Disk(HexCoord center, int radius)
    {
        for (int dq = -radius; dq <= radius; dq++)
        {
            for (int dr = Math.Max(-radius, -dq - radius); dr <= Math.Min(radius, -dq + radius); dr++)
            {
                yield return new HexCoord(center.Q + dq, center.R + dr);
            }
        }
    }

    public static IEnumerable<HexCoord> Hexagon(HexCoord center, int radius)
    {
        foreach (var hex in Disk(center, radius))
            yield return hex;
    }

    public static IEnumerable<HexCoord> Rectangle(HexCoord topLeft, int width, int height)
    {
        for (int dq = 0; dq < width; dq++)
        {
            for (int dr = 0; dr < height; dr++)
            {
                yield return new HexCoord(topLeft.Q + dq, topLeft.R + dr);
            }
        }
    }

    public static IEnumerable<HexCoord> Line(HexCoord a, HexCoord b)
    {
        int N = Distance(a, b);
        for (int i = 0; i <= N; i++)
        {
            yield return HexLerpRound(a, b, (1.0 / N) * i);
        }
    }

    // ==== Section : Utility ==== \\

    private static HexCoord HexLerpRound(HexCoord a, HexCoord b, double t)
    {
        double aq = a.Q + (b.Q - a.Q) * t;
        double ar = a.R + (b.R - a.R) * t;
        double as_ = -aq - ar;

        int rq = (int)Math.Round(aq);
        int rr = (int)Math.Round(ar);
        int rs = (int)Math.Round(as_);

        double dq = Math.Abs(rq - aq);
        double dr = Math.Abs(rr - ar);
        double ds = Math.Abs(rs - as_);

        if (dq > dr && dq > ds)
            rq = -rr - rs;
        else if (dr > ds)
            rr = -rq - rs;

        return new HexCoord(rq, rr);
    }
    
    // ==== Section : Equality and Hashing ==== \\
    public bool Equals(HexCoord other)
    {
        return Q == other.Q && R == other.R;
    }

    public override bool Equals(object obj)
    {
        return obj is HexCoord other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R);
    }
}

public abstract class HexTile
{
    public HexCoord Coordinate { get; private set; }
}
