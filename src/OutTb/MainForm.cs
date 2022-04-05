using System.IO.Pipes;
using System.Reflection;

namespace OutTb;

internal class MainForm : Form
{
    private readonly RichTextBox _outputTb;
    private readonly string[] _args;

    public MainForm(string[] args)
    {
        _args = args;

        Text = "Loading";
        WindowState = FormWindowState.Maximized;

        _outputTb = new RichTextBox
        {
            Multiline = true,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both,
            Dock = DockStyle.Fill,
            Enabled = false
        };

        Controls.Add(_outputTb);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_args.Length > 0 && _args[0] == "-p")
        {
            await LoadFromPipeAsync();
            return;
        }

        // file
        if (_args.Length == 1)
        {
            await LoadFromFileAsync();
            return;
        }

        LoadfromClipboard();
    }

    private async ValueTask LoadFromFileAsync()
    {
        var filePath = _args[0];
        if (!File.Exists(filePath))
        {
            SetContent(filePath, $"File {filePath} does not exist.");
            return;
        }

        var text = await File.ReadAllTextAsync(filePath);
        SetContent(Path.GetFileName(filePath), text);
    }

    private async ValueTask LoadFromPipeAsync()
    {
        if (_args.Length < 2)
        {
            SetContent("Redirect", "Error, client handle not specified.");
            return;
        }
        var pipeHandle = _args[1];
        await using var pipeStream = new AnonymousPipeClientStream(PipeDirection.In, pipeHandle);
        using var reader = new StreamReader(pipeStream);
        var text = await reader.ReadToEndAsync();

        SetContent("Redirect", text);
    }

    private void LoadfromClipboard()
    {
        var text = Clipboard.ContainsText() ? Clipboard.GetText() : "";
        SetContent("Clipboard", text);
    }

    private void SetContent(string source, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Text = $"{source} - No Content";
            _outputTb.Enabled = true;
            _outputTb.Text = "";
            _outputTb.Focus();
        }
        else
        {
            Text = $"{source} - {GetLineCount(text)} lines";
            _outputTb.Enabled = true;
            _outputTb.Text = text;
            _outputTb.Focus();
        }
    }

#pragma warning disable S1144 // Unused private types or members should be removed
    private void ScreenreaderAlert(string msg)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        MethodInfo raiseMethod = typeof(AccessibleObject).GetMethod("RaiseAutomationNotification");
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        if (raiseMethod != null)
        {
            raiseMethod.Invoke(AccessibilityObject, new object[3] {/*Other*/ 4, /*All*/ 2, msg });
        }
    }
#pragma warning restore S1144 // Unused private types or members should be removed

    private int GetLineCount(string src)
    {
        if (string.IsNullOrEmpty(src))
        {
            return 0;
        }

        int rc = src.Count(c => c == '\n') + 1;
        if (src.EndsWith('\n'))
        {
            rc--;
        }
        return rc;
    }

}