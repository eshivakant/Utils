public async Task InvokeAsync(HttpContext context)
    {
        // Store the original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a new memory stream to temporarily hold the response body
        using (var responseBody = new MemoryStream())
        {
            // Replace the original response body stream with the memory stream
            context.Response.Body = responseBody;

            // Call the next middleware in the pipeline
            await _next(context);

            // Read and log the response body
            await LogResponseAsync(context);

            // Copy the content from the memory stream back to the original response body stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogResponseAsync(HttpContext context)
    {
        // Reset the memory stream position to the beginning
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        // Read the response body from the memory stream
        var text = await new StreamReader(context.Response.Body).ReadToEndAsync();

        // Log the response body
        _logger.LogInformation($"HTTP Response: {text}");

        // Reset the stream position again for the subsequent middleware or the client to read it
        context.Response.Body.Seek(0, SeekOrigin.Begin);
    }
