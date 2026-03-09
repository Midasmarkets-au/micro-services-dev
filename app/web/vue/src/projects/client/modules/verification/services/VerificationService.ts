import { axiosInstance as axios } from "@/core/services/api.client";

const prefix = "api/v1/client/";

export default {
  codeVerify: async (formData: any) =>
    (await axios.post(prefix + "verification/verify-code ", formData)).data,

  checkClientAnswer: async (formData?: any) =>
    (await axios.post(prefix + "quiz/verification/step1", formData)).data,

  checkClientProfessionalAnswer: async (formData?: any) =>
    (await axios.post(prefix + "quiz/verification/step2", formData)).data,

  getMyReferralCode: async () =>
    (await axios.get("/api/v1/user/me/refercode")).data,

  getReferralInfoByReferralCode: async (referralCode: string) =>
    await axios.get(`/api/v1/referralcode/${referralCode}`),

  getVerification: async () => (await axios.get(prefix + "verification")).data,

  getExistingVerifications: async () =>
    (await axios.get(prefix + "verification/existing")).data,

  postVerificationStarted: async (formData: any) =>
    (await axios.post(prefix + "verification/started", formData)).data,

  postVerificationInfo: async (formData: any) =>
    (await axios.post(prefix + "verification/info", formData)).data,

  postVerificationAgreement: async (formData: any) =>
    (await axios.post(prefix + "verification/agreement", formData)).data,

  postVerificationFinancial: async (formData: any) =>
    (await axios.post(prefix + "verification/financial", formData)).data,

  submitVerification: async () =>
    (await axios.put(prefix + "verification/submit")).data,

  submitNewVerification: async () =>
    (await axios.post(prefix + "verification/new")).data,

  postVerificationQuiz: async (formData: any) =>
    (await axios.post(prefix + "verification/quiz", formData)).data,

  uploadDocumentsForVerification: async (type, file: any) =>
    (
      await axios.post(`${prefix}verification/document/upload`, file, {
        params: { type },
        // headers: {
        //   "Content-Type": "multipart/form-data",
        // },
      })
    ).data,

  verificationSubmitWithFiles: async (formData?: any) =>
    (await axios.post("api/v2/client/verification/document/submit", formData))
      .data,

  verificationSubmitWithSlicedFiles: async (formData?: any) =>
    (
      await axios.post(
        "api/v2/client/verification/document/chunk/submit",
        formData
      )
    ).data,
  // uploadDocumentsForVerification: async (type, file: any) => {
  //   const formData = new FormData();
  //   formData.append("file", file);

  //   return (
  //     await axios.post(`${prefix}verification/document/upload`, formData, {
  //       params: { type },
  //       // No need to set the Content-Type header; Axios will set it automatically
  //     })
  //   ).data;
  // },
};
