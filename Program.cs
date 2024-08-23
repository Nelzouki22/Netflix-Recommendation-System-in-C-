using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    // Movie data structure
    class Movie
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    // Load movies from CSV
    static List<Movie> LoadMovies(string filePath)
    {
        var movies = new List<Movie>();
        var lines = File.ReadAllLines(filePath);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var columns = lines[i].Split(',');
            if (columns.Length >= 2)
            {
                movies.Add(new Movie { Title = columns[0], Description = columns[1] });
            }
        }
        return movies;
    }

    // Extract keywords from description
    static Dictionary<string, int> ExtractKeywords(string text)
    {
        var keywords = new Dictionary<string, int>();
        var words = Regex.Split(text.ToLower(), @"\W+");
        foreach (var word in words)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                if (keywords.ContainsKey(word))
                    keywords[word]++;
                else
                    keywords[word] = 1;
            }
        }
        return keywords;
    }

    // Calculate cosine similarity
    static double CosineSimilarity(Dictionary<string, int> vec1, Dictionary<string, int> vec2)
    {
        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        foreach (var pair in vec1)
        {
            if (vec2.ContainsKey(pair.Key))
            {
                dotProduct += pair.Value * vec2[pair.Key];
            }
            magnitude1 += pair.Value * pair.Value;
        }

        foreach (var value in vec2.Values)
        {
            magnitude2 += value * value;
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        return dotProduct / (magnitude1 * magnitude2);
    }

    // Recommend movies based on similarity
    static List<string> RecommendMovies(string title, List<Movie> movies)
    {
        var recommendations = new List<Tuple<double, string>>();
        var targetKeywords = ExtractKeywords(title);

        foreach (var movie in movies)
        {
            if (movie.Title != title)
            {
                var movieKeywords = ExtractKeywords(movie.Description);
                var score = CosineSimilarity(targetKeywords, movieKeywords);
                recommendations.Add(Tuple.Create(score, movie.Title));
            }
        }

        return recommendations.OrderByDescending(x => x.Item1).Select(x => x.Item2).ToList();
    }

    static void Main()
    {
        string filePath = "netflix_titles.csv";
        var movies = LoadMovies(filePath);

        string inputTitle = "Some Movie";
        var recommendations = RecommendMovies(inputTitle, movies);

        Console.WriteLine("Recommendations:");
        foreach (var rec in recommendations)
        {
            Console.WriteLine(rec);
        }
    }
}

