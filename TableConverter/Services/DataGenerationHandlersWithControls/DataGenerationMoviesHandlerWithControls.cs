using Avalonia.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;
using Avalonia.Layout;
using System.Linq;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationMoviesHandlerWithControls : DataGenerationMoviesHandler, IInitializeControls
    {
        public Collection<Control> OptionsControls { get; set; } = new();

        public void InitializeControls()
        {
            var movie_genre_label = new TextBlock()
            {
                Text = "Genre:",
                VerticalAlignment = VerticalAlignment.Center,
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

                    Options!.MovieGenres = movie_genres.ToArray();

                    Options!.MovieGenre = Options!.MovieGenres.First();
                }
            }

            var movie_genres_combo_box = new ComboBox()
            {
                ItemsSource = Options!.MovieGenres,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Center,
            };

            movie_genres_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    Options!.MovieGenre = item;
                }
            };

            OptionsControls.Add(movie_genre_label);
            OptionsControls.Add(movie_genres_combo_box);
        }
    }
}
