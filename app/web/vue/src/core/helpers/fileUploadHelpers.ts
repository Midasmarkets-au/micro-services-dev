export const isTradeBcr = () => {
  //check if domain is tradebcr.com
  return true;
  return window.location.hostname.includes("tradebcr.com");
};

export const sliceFile = (file, chunkSize = 512000) => {
  console.log("chunkSize", chunkSize);
  console.log("File size:", file.size);
  console.log(
    "chunkSize:",
    chunkSize,
    "Expected Chunks:",
    Math.ceil(file.size / chunkSize)
  );
  const chunks: Blob[] = [];

  let start = 0;
  while (start < file.size) {
    const end = Math.min(start + chunkSize, file.size);
    const chunk = file.slice(start, end);
    chunks.push(chunk);
    console.log(`Chunk ${chunks.length}: ${chunk.size} bytes`);
    start = end;
  }
  console.log("chunk size", chunks.length);
  return chunks;
};
