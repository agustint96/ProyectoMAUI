using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PhoneWords
{
    public partial class MainPage : ContentPage
    {
        const int MaxLives = 7;

        string secret = string.Empty;         // palabra normalizada en mayúsculas
        char[] displayChars = Array.Empty<char>();
        HashSet<char> guessed = new();
        int lives;

        public MainPage()
        {
            InitializeComponent();
            ResetToSetup();
            StartTitleAnimation();  // ← agregar
        }

        void OnStartClicked(object sender, EventArgs e) => StartGame();

        void StartGame()
        {
            SetupMessage.IsVisible = false;

            var raw = (SecretEntry.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(raw))
            {
                SetupMessage.Text = "Debes ingresar una palabra.";
                SetupMessage.IsVisible = true;

                return;
            }

            // Normalizar: no es case sensitive -> trabajar en mayúsculas internamente
            secret = raw.ToUpperInvariant();

            // Inicializar display: letras -> '_', espacios y símbolos -> mostrarlos
            displayChars = secret
                .Select(c => char.IsLetter(c) ? '_' : c)
                .ToArray();

            guessed.Clear();
            lives = MaxLives;

            // UI
            SetupPanel.IsVisible = false;
            FooterPanel.IsVisible = false;  // ← agregar esta línea
            StopTitleAnimation();        // ← agregar
            TitleLabel.IsVisible = false;  // ← agregar
            GamePanel.IsVisible = true;
            ResultLabel.IsVisible = false;
            GuessEntry.Text = string.Empty;
            GuessEntry.IsEnabled = true;
            GuessEntry.Focus();

            UpdateDisplay();
        }

        void OnGuessSubmitted(object sender, EventArgs e) => ProcessGuess();
        void OnGuessClicked(object sender, EventArgs e) => ProcessGuess();

        void ProcessGuess()
        {
            var input = (GuessEntry.Text ?? string.Empty).Trim().ToUpperInvariant();
            GuessEntry.Text = string.Empty;

            if (string.IsNullOrEmpty(input))
            {
                ShowResult("Ingresá una letra.", Colors.Orange);
                return;
            }

            char letter = input[0];
            if (!char.IsLetter(letter))
            {
                ShowResult("Ingresá una letra válida (A-Z).", Colors.Orange);
                return;
            }

            if (guessed.Contains(letter))
            {
                ShowResult($"Ya probaste la letra '{letter}'.", Colors.Orange);
                return;
            }

            guessed.Add(letter);

            if (secret.Contains(letter))
            {
                // Revelar todas las posiciones de la letra
                for (int i = 0; i < secret.Length; i++)
                {
                    if (secret[i] == letter)
                        displayChars[i] = letter;
                }

                ShowResult($"¡Bien! La letra '{letter}' está en la palabra.", Colors.Green);
            }
            else
            {
                lives--;
                ShowResult($"La letra '{letter}' no está. Perdiste una vida.", Colors.Red);
            }

            UpdateDisplay();
            CheckEndConditions();
            GuessEntry.Focus();
        }

        void ShowResult(string text, Color color)
        {
            ResultLabel.Text = text;
            ResultLabel.TextColor = color;
            ResultLabel.IsVisible = true;
        }

        void UpdateDisplay()
        {
            // Mostrar con espacios entre caracteres para legibilidad
            WordDisplay.Text = string.Join(" ", displayChars);

            // Obtener color 'Tertiary' de recursos si existe (dark violet)
            Color heartColor = Colors.Purple;
            if (Application.Current?.Resources?.ContainsKey("Tertiary") == true)
                heartColor = (Color)Application.Current.Resources["Tertiary"];
            else if (Application.Current?.Resources?.ContainsKey("Primary") == true)
                heartColor = (Color)Application.Current.Resources["Primary"];

            // Mostrar sólo corazones llenos: uno por cada vida restante
            var formatted = new FormattedString();
            for (int i = 0; i < lives; i++)
            {
                formatted.Spans.Add(new Span
                {
                    Text = "❤ ",
                    TextColor = heartColor,
                    FontSize = 28
                });
            }

            LivesLabel.FormattedText = formatted;

            // Mostrar únicamente las letras erróneas (las que NO están en la palabra secreta)
            var wrong = guessed
                .Where(c => !secret.Contains(c))
                .OrderBy(c => c)
                .ToArray();

            GuessedLabel.Text = wrong.Length > 0
                ? $"Letras erróneas: {string.Join(", ", wrong)}"
                : "Letras erróneas: ninguna";
        }

        void CheckEndConditions()
        {
            if (!displayChars.Any(c => c == '_'))
            {
                // Ganó
                ShowResult("¡Ganaste! Palabra completada.", Colors.Green);
                EndGame();
            }
            else if (lives <= 0)
            {
                // Perdió
                ShowResult($"Perdiste. La palabra era: {secret}", Colors.Red);
                // Revelar palabra completa
                for (int i = 0; i < secret.Length; i++)
                    displayChars[i] = secret[i];
                UpdateDisplay();
                EndGame();
            }
        }

        void EndGame()
        {
            GuessEntry.IsEnabled = false;

        }


        void OnResetClicked(object sender, EventArgs e)
        {
            // Reinicia con la misma palabra
            guessed.Clear();
            displayChars = secret.Select(c => char.IsLetter(c) ? '_' : c).ToArray();
            lives = MaxLives;
            ResultLabel.IsVisible = false;
            GuessEntry.IsEnabled = true;
            GuessEntry.Text = string.Empty;
            UpdateDisplay();
            GuessEntry.Focus();
        }

        void OnResetToSetupClicked(object sender, EventArgs e) => ResetToSetup();

        void ResetToSetup()
        {
            secret = string.Empty;
            displayChars = Array.Empty<char>();
            guessed.Clear();
            lives = MaxLives;
            SecretEntry.Text = string.Empty;
            SetupMessage.IsVisible = false;
            SetupPanel.IsVisible = true;
            GamePanel.IsVisible = false;
            FooterPanel.IsVisible = true;  // ← agregar esta línea
            
            TitleLabel.IsVisible = true;  // ← agregar
            StartTitleAnimation();  // ← agregar

        }







        const string TitleText = "AHORCADO";
        bool _titleAnimating = false;

        void StartTitleAnimation()
        {
            _titleAnimating = true;
            _ = AnimateTitleAsync();
        }

        void StopTitleAnimation()
        {
            _titleAnimating = false;
        }

        async Task AnimateTitleAsync()
        {
            var rng = new Random();
            var chars = TitleText.ToCharArray();

            while (_titleAnimating)
            {
                // Empezar con todo oculto
                var hidden = new bool[chars.Length];
                for (int i = 0; i < chars.Length; i++)
                    hidden[i] = true;

                TitleLabel.Text = new string(chars.Select((c, i) => hidden[i] ? '_' : c).ToArray());
                await Task.Delay(800);

                // Ir revelando letra por letra de izquierda a derecha
                for (int i = 0; i < chars.Length; i++)
                {
                    if (!_titleAnimating) return;

                    hidden[i] = false;
                    TitleLabel.Text = new string(chars.Select((c, j) => hidden[j] ? '_' : c).ToArray());
                    await Task.Delay(rng.Next(300, 700));
                }

                // Quedarse completo un momento
                await Task.Delay(1500);

                // Volver a ocultar todo y repetir
                await Task.Delay(500);
            }

            TitleLabel.Text = TitleText;
        }
    }
}
