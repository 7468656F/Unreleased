using System.Diagnostics;

namespace Unreleased.Downloader.Transcoding;

public class Transcoder
{
    /// <summary>
    /// Transcodes a byte array to an MP3 file and saves it to the specified output path.
    /// </summary>
    /// <param name="data">The byte array containing the input data to be transcoded.</param>
    /// <param name="fileName">The name of the output file (without extension).</param>
    /// <param name="outputPath">The directory where the transcoded file will be saved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the full path
    /// to the saved MP3 file.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if the ffmpeg process fails with a non-zero exit code.
    /// </exception>
    public static async Task<string> TranscodeAndSaveBytes(byte[] data, string fileName, string outputPath)
    {
        var inputFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bin");
        var outputFilePath = Path.Combine(outputPath, fileName + ".mp3");
        
        await File.WriteAllBytesAsync(inputFilePath, data);

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-y -i \"{inputFilePath}\" \"{outputFilePath}\"", // TODO: sanitize inputs
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        Debug.WriteLine("Transcoder: Starting ffmpeg process...");
        process.Start();

        var stderr = await process.StandardError.ReadToEndAsync();
        var stdout = await process.StandardOutput.ReadToEndAsync();
        Debug.WriteLine("Transcoder: ffmpeg process finished.");
        Debug.WriteLine($"Transcoder: StdErr: {stderr}");
        Debug.WriteLine($"Transcoder: StdOut: {stdout}");

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"ffmpeg failed with exit code {process.ExitCode}:\n{stderr}");
        }
        
        File.Delete(inputFilePath);
        Debug.WriteLine("Transcoder: Temporary input file deleted.");
        
        var formattedOutputFilePath = Path.GetFullPath(outputFilePath);
        Debug.WriteLine("Transcoder: Transcoding completed successfully. Output file at " + formattedOutputFilePath);
        return formattedOutputFilePath;
    }
}