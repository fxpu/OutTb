using System.Diagnostics;
using System.IO.Pipes;

try
{
    await using var pipeStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
    var pipeHandle = pipeStream.GetClientHandleAsString();
    Process.Start(new ProcessStartInfo("OutTb.exe")
        {
            UseShellExecute = false,
            Arguments = $"-p {pipeHandle}"
        });
    pipeStream.DisposeLocalCopyOfClientHandle();

    // copy stdin to pipe
    using var stdIn = Console.OpenStandardInput();
    await stdIn.CopyToAsync(pipeStream);

#pragma warning disable CA1416 // Validate platform compatibility
    pipeStream.WaitForPipeDrain();
#pragma warning restore CA1416 // Validate platform compatibility

}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}