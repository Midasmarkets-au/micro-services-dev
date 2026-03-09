import QRCode from "qrcodejs2";

export interface QrCodeOptions {
  width?: number;
  height?: number;
  colorDark?: string;
  colorLight?: string;
  correctLevel?: any;
}

export const generateQrCodeImageSrc = async (
  text: string,
  options: QrCodeOptions = {}
): Promise<string> => {
  return new Promise((resolve, reject) => {
    try {
      const defaultOptions: QrCodeOptions = {
        width: 256,
        height: 256,
        colorDark: "#000000",
        colorLight: "#ffffff",
        correctLevel: QRCode.CorrectLevel.H,
      };

      const qrCodeOptions = { ...defaultOptions, ...options };

      const tempDiv = document.createElement("div");
      tempDiv.style.position = "absolute";
      tempDiv.style.left = "-9999px";
      document.body.appendChild(tempDiv);

      const qrCode = new QRCode(tempDiv, {
        text,
        ...qrCodeOptions,
      });

      // Wait for QR code generation
      setTimeout(() => {
        const canvas = tempDiv.querySelector("canvas");
        if (canvas) {
          const dataUrl = canvas.toDataURL("image/png");
          document.body.removeChild(tempDiv);
          resolve(dataUrl);
        } else {
          document.body.removeChild(tempDiv);
          reject(new Error("Failed to generate QR code"));
        }
      }, 100);
    } catch (error) {
      reject(error);
    }
  });
};
