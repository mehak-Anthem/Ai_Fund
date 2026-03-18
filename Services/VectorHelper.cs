namespace Ai_Fund.Services;

public static class VectorHelper
{
    public static double CosineSimilarity(float[] v1, float[] v2)
    {
        if (v1.Length != v2.Length)
            throw new ArgumentException("Vectors must have the same length");

        double dot = 0, mag1 = 0, mag2 = 0;

        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += Math.Pow(v1[i], 2);
            mag2 += Math.Pow(v2[i], 2);
        }

        return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
    }
}
