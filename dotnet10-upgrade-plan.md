# Upgrading HypnoTech Solution and Sites to .NET 10 (LTS)

This document outlines the strategy for upgrading the shared HypnoTech engine and website projects from .NET Core 3.1 to **.NET 10 (LTS)**, using a **Test-First** strategy to verify existing behavior before making framework changes, and migrating image processing to **SkiaSharp** (MIT).

---

## Key Platform & Dependency Decisions

### 1. Cross-Platform Image Manipulation (System.Drawing vs. Alternatives)
- **Status**: `System.Drawing.Common` is strictly Windows-only in .NET 10. Attempting to run it on Linux/Docker will throw a `PlatformNotSupportedException`.
- **Decision**: Remove `SixLabors.ImageSharp` and migrate to **SkiaSharp** (MIT licensed, completely free, highly performant, and fully cross-platform).
- **ImageExtensions Rewrite Code**:
  ```csharp
  using System.IO;
  using SkiaSharp;

  namespace HypnoTech.Cms.Extensions
  {
      public static class ImageExtensions
      {
          public static byte[] Scale(this byte[] fileBytes, int maxWidth, int maxHeight)
          {
              using (var codec = SKCodec.Create(new MemoryStream(fileBytes)))
              {
                  if (codec == null) return fileBytes;

                  var info = codec.Info;
                  if (info.Width <= maxWidth && info.Height <= maxHeight)
                  {
                      return fileBytes;
                  }

                  int width = info.Width;
                  int height = info.Height;
                  int newWidth = 0;
                  int newHeight = 0;
                  double ratio;

                  if (width > maxWidth)
                  {
                      ratio = width / (double)maxWidth;
                      newWidth = maxWidth;
                      newHeight = (int)(height / ratio);
                  }
                  else if (height > maxHeight)
                  {
                      ratio = height / (double)maxHeight;
                      newHeight = maxHeight;
                      newWidth = (int)(width / ratio);
                  }

                  if (newHeight < 1) newHeight = 1;
                  if (newWidth < 1) newWidth = 1;

                  if (newWidth > maxWidth)
                  {
                      ratio = newWidth / (double)maxWidth;
                      newWidth = maxWidth;
                      newHeight = (int)(newHeight / ratio);
                  }
                  else if (newHeight > maxHeight)
                  {
                      ratio = newHeight / (double)maxHeight;
                      newHeight = maxHeight;
                      newWidth = (int)(newWidth / ratio);
                  }

                  if (newHeight < 1) newHeight = 1;
                  if (newWidth < 1) newWidth = 1;

                  using (var originalBitmap = SKBitmap.Decode(codec))
                  {
                      if (originalBitmap == null) return fileBytes;

                      var imageInfo = new SKImageInfo(newWidth, newHeight, originalBitmap.ColorType, originalBitmap.AlphaType);
                      using (var resizedBitmap = originalBitmap.Resize(imageInfo, SKFilterQuality.High))
                      {
                          if (resizedBitmap == null) return fileBytes;

                          using (var image = SKImage.FromBitmap(resizedBitmap))
                          using (var data = image.Encode(codec.EncodedFormat, 100))
                          {
                              return data.ToArray();
                        }
                    }
                }
            }
        }
    }
}
  ```

### 2. MongoDB Driver Selection for Server 4.0.3
- **Status**: The production database is running MongoDB `4.0.3`. The latest MongoDB C# Driver (v3.5+) drops support for MongoDB Server versions older than 4.2.
- **Decision**: Target **`MongoDB.Driver` version `2.30.0`** or **`3.4.0`**. These versions include necessary runtime adaptations for modern .NET environments and C# 14 compiler patterns while retaining official support for older MongoDB 4.0.x server deployments.
- **Local Testing**: Spin up a MongoDB 4.0.3 container in Docker for local testing to match the production database version.

### 3. Preserving Legacy PayPal Integrations
- **Status**: The official PayPal .NET SDK is fully deprecated and unsupported.
- **Decision**: Retain the existing `PayPal` SDK package (`2.0.0-rc2`) and legacy v1 integration as-is to prevent scope creep. Since .NET 10 is backwards-compatible with .NET Standard 2.0 packages, the legacy SDK should compile and function without modification.

### 4. HTTP Stream Rewriting & SPA Middleware Stability
- **Status**: The stream rewrite and async response patterns adapted during the .NET Core 2.x to 3.1 migration remain fully compatible and stable in .NET 10.
- **Decision**: Keep the `AllowSynchronousIO` configuration settings, but no rewrite of custom stream handlers (like `AjaxPartialViewResultExecutor.cs`) is expected to be needed.

---

## Upgrade Timeline (Phases)

### Phase 1: Pre-Upgrade Baseline Tests (Test-First)
Write all unit, integration, and E2E browser tests *before* performing any upgrades.
1. **Docker Setup**: Spin up a local MongoDB `4.0.3` container to mimic the production server.
2. **Integration Tests**: Create a new test project (running under `netcoreapp3.1` or `netstandard2.1`) targeting the DAL repositories to assert database queries, reads, and writes.
3. **E2E Playwright Tests**: Install Playwright and write browser tests mapping major frontend flows (page navigation, user forms, cart/checkout simulation, admin page logging) on the local .NET Core 3.1 instances.
4. **Baseline Run**: Run all tests and ensure 100% pass rate.

### Phase 2: SkiaSharp Migration
1. Remove `SixLabors.ImageSharp` dependency.
2. Add `SkiaSharp` and `SkiaSharp.NativeAssets.Linux` (for Linux deployment compatibility).
3. Apply the rewritten `Scale` method in `ImageExtensions.cs`.
4. Run integration tests to verify that image scaling still returns correct byte arrays and proportions under .NET 3.1.

### Phase 3: Upgrade to .NET 10
1. Upgrade targets from `netcoreapp3.1` to `net10.0`.
2. Update dependencies (`MongoDB.Driver` to `2.30.0` or `3.4.0` to preserve Server 4.0.3 support).
3. Keep the legacy `PayPal` package (`2.0.0-rc2`) as-is.
4. Use the .NET Upgrade Assistant to automate project conversion:
   ```bash
   dotnet tool install -g upgrade-assistant
   upgrade-assistant upgrade engine/HypnoTech.slnx --non-interactive
   ```

### Phase 4: Final Verification & Fixes
1. Run all Unit & Integration tests on the new .NET 10 target. Fix any compilation or query evaluation errors.
2. Start the updated site and run the Playwright test suite to confirm all user-facing features work identically.
