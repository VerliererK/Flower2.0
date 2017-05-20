namespace ImageCaption.Services
{
	using System.IO;
	using System.Threading.Tasks;
    using Microsoft.ProjectOxford.Vision.Contract;

    /// <summary>
    /// An interface that defines the contract with the caption service.
    /// </summary>
    internal interface ICaptionService
	{
		/// <summary>
		/// Gets the caption of an image stream.
		/// </summary>
		/// <param name="stream">The stream to an image.</param>
		/// <returns>Description if caption found, null otherwise.</returns>
		Task<string> GetCaptionAsync(Stream stream);

		/// <summary>
		/// Gets the caption of an image URL.
		/// </summary>
		/// <param name="url">The URL to an image.</param>
		/// <returns>Description if caption found, null otherwise.</returns>
		Task<string> GetCaptionAsync(string url);

		/// <summary>
		/// Gets the AnalysisResult of an image URL.
		/// </summary>
		/// <param name="stream">The stream to an image.</param>
		/// <returns>Description if AnalysisResult found, null otherwise.</returns>
		Task<AnalysisResult> GetAnalysisResultAsync(Stream stream);

		/// <summary>
		/// Gets the AnalysisResult of an image URL.
		/// </summary>
		/// <param name="url">The URL to an image.</param>
		/// <returns>Description if AnalysisResult found, null otherwise.</returns>
		Task<AnalysisResult> GetAnalysisResultAsync(string url);
	}
}
