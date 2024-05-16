using Avalonia.Controls;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;
using Avalonia.Layout;
using Avalonia.Media;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationMoviesHandler : DataGenerationTypeHandlerAbstract
    {
        private string[]? MovieGenres { get; set; }

        private string? MovieGenre { get; set; }

        public override void InitializeOptionsControls()
        {
            var movie_genre_label = new Label()
            {
                Content = "Genre:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            using (var reader = DbConnection.ExecuteCommand(
                "SELECT GENRE FROM MOVIES_GENRES_TABLE;"
            ))
            {
                if (reader.HasRows)
                {
                    var movie_genres = new List<string>();

                    while (reader.Read())
                    {
                        movie_genres.Add(reader.GetString(0));
                    }

                    movie_genres.Sort();

                    MovieGenres = movie_genres.ToArray();

                    MovieGenre = MovieGenres.First();
                }
            }

            var movie_genres_combo_box = new ComboBox()
            {
                ItemsSource = MovieGenres,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Stretch,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            movie_genres_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    MovieGenre = item;
                }
            };

            OptionsControls.Add(movie_genre_label);
            OptionsControls.Add(movie_genres_combo_box);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                using (var reader = DbConnection.ExecuteCommand(
                    $"SELECT M.NAME FROM MOVIES_TABLE M INNER JOIN MOVIES_TO_GENRES_MAP_TABLE MG ON M.ID = MG.MOVIE_ID INNER JOIN MOVIES_GENRES_TABLE G ON MG.GENRE = G.GENRE WHERE G.GENRE = '{MovieGenre}'" + 
                    $"AND M.ID IN (SELECT ID FROM MOVIES_TABLE WHERE ID IN (SELECT MG.MOVIE_ID FROM MOVIES_TO_GENRES_MAP_TABLE MG JOIN MOVIES_GENRES_TABLE G ON MG.GENRE = G.GENRE WHERE G.GENRE = '{MovieGenre}') ORDER BY RANDOM() LIMIT {rows});"
                ))
                {
                    if (!reader.HasRows)
                    {
                        throw new SQLiteException("No rows returned from the database.");
                    }

                    long i = 0;

                    while (reader.Read())
                    {
                        data[i++] = CheckBlank(() => reader.GetString(0), blanks_percentage);
                    }

                    if (i < rows)
                    {
                        for (long j = i; j < rows; j++)
                        {
                            data[j] = data[j - i];
                        }
                    }
                };

                return data;
            });
        }
    }
}
