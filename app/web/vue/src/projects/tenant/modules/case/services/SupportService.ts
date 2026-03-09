import {
  axiosInstance as axios,
  paramsSerializerForDotnet,
} from "@/core/services/api.client";
const prefix = "api/v1/tenant/";

export default {
  queryCasesList: async (criteria?: any) =>
    (await axios.get(prefix + "case", { params: criteria })).data,

  queryCaseById: async (id: number) =>
    (await axios.get(prefix + `case/${id}`)).data,

  claimCase: async (id: number) =>
    (await axios.post(prefix + `case/${id}/claim`)).data,

  closeCase: async (id: number) =>
    (await axios.put(prefix + `case/${id}/close`)).data,

  uploadCaseFile: async (type, file: any) =>
    (
      await axios.post("api/v1/tenant/upload", file, {
        params: { type },
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
    ).data,

  replyCaseById: async (id: number, formData: any) =>
    (await axios.post(prefix + "case/" + id + "/reply", formData)).data,

  getImagesWithGuid: async (guid: string) => {
    const response = await axios.get(`${prefix}media/${guid}`, {
      responseType: "blob",
    });
    const imageBlob = new Blob([response.data], { type: "image/jpeg" }); // Adjust the MIME type as necessary
    const imageBlobUrl = URL.createObjectURL(imageBlob);
    return imageBlobUrl;
  },

  translateCaseText: async (id: number, formData: any) =>
    (await axios.put(prefix + "case/" + id + "/translate", formData)).data,
};
