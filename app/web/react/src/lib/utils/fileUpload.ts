/**
 * 文件上传工具函数
 */

// 默认切片大小：512KB
const DEFAULT_CHUNK_SIZE = 512 * 1024;

/**
 * 将文件切片
 * @param file 要切片的文件
 * @param chunkSize 每个切片的大小（字节），默认 512KB
 * @returns 切片数组
 */
export function sliceFile(file: File, chunkSize = DEFAULT_CHUNK_SIZE): Blob[] {
  const chunks: Blob[] = [];
  let start = 0;
  
  while (start < file.size) {
    const end = Math.min(start + chunkSize, file.size);
    const chunk = file.slice(start, end);
    chunks.push(chunk);
    start = end;
  }
  
  return chunks;
}

/**
 * 生成唯一的文件ID
 * @param userId 用户ID
 * @returns 唯一文件ID
 */
export function generateFieldId(userId?: string): string {
  const timestamp = new Date().toISOString().replace(/[-:.]/g, '');
  const random = Math.floor(10000 + Math.random() * 90000);
  return `${userId || 'user'}-${timestamp}-${random}`;
}

/**
 * 文件切片上传信息
 */
export interface ChunkUploadInfo {
  fieldId: string;
  fileName: string;
  contentType: string;
  type: string;
  chunkIndex: number;
  chunkSize: number;
  totalChunks: number;
  totalSize: number;
  chunk: Blob;
}

/**
 * 准备文件切片上传数据
 * @param file 文件
 * @param type 文件类型（如 id_front, id_back, address）
 * @param userId 用户ID
 * @returns 切片上传信息数组
 */
export function prepareChunkedUpload(
  file: File,
  type: string,
  userId?: string
): ChunkUploadInfo[] {
  const chunks = sliceFile(file);
  const fieldId = generateFieldId(userId);
  
  return chunks.map((chunk, index) => ({
    fieldId,
    fileName: file.name,
    contentType: file.type,
    type,
    chunkIndex: index,
    chunkSize: chunk.size,
    totalChunks: chunks.length,
    totalSize: file.size,
    chunk,
  }));
}

/**
 * 创建切片上传的 FormData
 */
export function createChunkFormData(info: ChunkUploadInfo): FormData {
  const formData = new FormData();
  formData.append('FieldId', info.fieldId);
  formData.append('FileName', info.fileName);
  formData.append('ContentType', info.contentType);
  formData.append('Type', info.type);
  formData.append('ChunkIndex', info.chunkIndex.toString());
  formData.append('ChunkSize', info.chunkSize.toString());
  formData.append('TotalChunks', info.totalChunks.toString());
  formData.append('TotalSize', info.totalSize.toString());
  formData.append('File', info.chunk);
  return formData;
}

/**
 * 创建合并请求的 FormData
 */
export function createMergeFormData(
  fieldId: string,
  fileName: string,
  contentType: string,
  type: string,
  totalChunks: number
): FormData {
  const formData = new FormData();
  formData.append('FieldId', fieldId);
  formData.append('FileName', fileName);
  formData.append('ContentType', contentType);
  formData.append('Type', type);
  formData.append('TotalChunks', totalChunks.toString());
  return formData;
}
