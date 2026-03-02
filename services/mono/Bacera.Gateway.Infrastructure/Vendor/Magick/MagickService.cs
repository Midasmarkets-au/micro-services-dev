using ImageMagick;

namespace Bacera.Gateway.Vendor.Magick;

public static class MagickService
{
    private const uint MaxImageWidth = 1920;
    private const uint MaxImageHeight = 1080;
    private const uint JpegQuality = 75;

    public static async Task CompressImageAsync(Stream stream, uint maxWidth = MaxImageWidth,
        uint maxHeight = MaxImageHeight, uint quality = JpegQuality)
    {
        using var image = new MagickImage(stream);

        // Determine if the original image has transparency
        var (newWidth, newHeight) = CalculateAspectRatioDimensions(
            originalWidth: image.Width,
            originalHeight: image.Height,
            maxWidth: maxWidth,
            maxHeight: maxHeight
        );

        if (newWidth != image.Width || newHeight != image.Height)
        {
            // Use Lanczos filter for high-quality resizing
            image.FilterType = FilterType.Lanczos;

            var geometry = new MagickGeometry((uint)newWidth, (uint)newHeight)
            {
                IgnoreAspectRatio = false,
                FillArea = false
            };

            image.Resize(geometry);
        }

        // Choose the appropriate format and optimization
        OptimizeImage(image, quality);

        // Save image to original stream
        stream.Position = 0;
        stream.SetLength(0);
        await image.WriteAsync(stream);
        stream.Position = 0;
    }

    private static (int width, int height) CalculateAspectRatioDimensions(
        uint originalWidth, // Changed from int to uint to match MagickImage properties
        uint originalHeight, // Changed from int to uint to match MagickImage properties
        uint maxWidth,
        uint maxHeight)
    {
        // Convert maxWidth and maxHeight to uint for consistent comparison

        // If image is already smaller than max dimensions, keep original size
        if (originalWidth <= maxWidth && originalHeight <= maxHeight)
        {
            // Cast back to int when returning since that's what our method expects
            return ((int)originalWidth, (int)originalHeight);
        }

        // Calculate the aspect ratios using double for precision
        var widthRatio = (double)maxWidth / originalWidth;
        var heightRatio = (double)maxHeight / originalHeight;
        var ratio = Math.Min(widthRatio, heightRatio);

        // Calculate new dimensions and ensure they're at least 1 pixel
        // Cast back to int for the return value
        var newWidth = Math.Max(1, (int)(originalWidth * ratio));
        var newHeight = Math.Max(1, (int)(originalHeight * ratio));

        return (newWidth, newHeight);
    }

    private static void OptimizeImage(MagickImage image, uint quality)
    {
        // JPEG optimization settings
        image.Format = MagickFormat.Jpeg;
        image.Quality = quality;

        // Enable progressive JPEG using compression settings
        image.Settings.SetDefine(MagickFormat.Jpeg, "interlace", "JPEG");

        // Strip all metadata to reduce file size
        image.Strip();

        // Additional JPEG-specific optimizations
        image.Settings.SetDefine(MagickFormat.Jpeg, "sampling-factor", "4:2:0");
        image.Settings.SetDefine(MagickFormat.Jpeg, "optimize-coding", "true");
    }
}