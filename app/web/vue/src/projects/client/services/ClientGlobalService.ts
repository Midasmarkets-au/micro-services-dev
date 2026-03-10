import i18n from "@/core/plugins/i18n";
import { axiosInstance as axios } from "@/core/services/api.client";
import { ServiceMapType } from "@/core/types/ServiceTypes";

const { t } = i18n.global;

const prefix = "api/v1/client/";
const prefixV2 = "api/v2/client/";

const userPrefix = "api/v1/user/";

const getServices = async (criteria?: any) =>
  (await axios.get(prefix + "trade/service", { params: criteria })).data;

const getServiceMap: () => Promise<ServiceMapType> = async function () {
  const map = (await getServices())
    // .filter((service) => service.isAllowAccountCreation === 1)
    .reduce((map: any, service: any) => {
      map[service.id] = {
        serverName: service.name,
        platform: service.platform,
        platformName: t(`type.platform.${service.platform}`),
      };
      return map;
    }, {});
  return map;
};

export default {
  getServices,
  getServiceMap,

  getMe: async () => (await axios.get(userPrefix + "me")).data,

  twoFaEnableCode: async (formData?: any) =>
    (await axios.put(userPrefix + "enable-2fa", formData)).data,

  twoFaDisableCode: async (formData?: any) =>
    (await axios.put(userPrefix + "disable-2fa", formData)).data,

  get2FaDisableCode: async () =>
    (await axios.put(userPrefix + "disable-2fa/code")).data,

  get2FaEnableCode: async () =>
    (await axios.put(userPrefix + "enable-2fa/code")).data,

  confirm2FaEnableCode: async (formData: any) =>
    (await axios.post(userPrefix + "enable-2fa/confirm", formData)).data,

  confirm2FaDisableCode: async (formData: any) =>
    (await axios.post(userPrefix + "disable-2fa/confirm", formData)).data,

  InsertNoShowEventId: async (key: string) =>
    (await axios.put(prefix + "event/" + key + "/last-check")).data,

  getEventByKey: async (key: string) =>
    (await axios.get(prefix + "event/" + key)).data,

  getEventList: async (criteria?: any) =>
    (await axios.get(prefix + "event", { params: criteria })).data,

  uploadFile: async (file: any, type?: string) =>
    (await axios.post(prefix + "upload", file, { params: { type } })).data,

  deleteMediaFileByGuid: async (guid: string) =>
    (await axios.delete(prefix + "media/" + guid)).data,

  getMediaFileList: async (criteria?: any) =>
    (await axios.get(prefix + "media/list", { params: criteria })).data,

  createLead: async (data: any) => (await axios.post("api/v1/lead", data)).data,

  uploadUserAvatar: async (data: any) =>
    (await axios.post("api/v1/user/profile/avatar", data)).data,

  updateUserProfile: async (formData: any) =>
    (await axios.put("api/v1/user/profile", formData)).data,

  getUserPaymentInfo: async () =>
    (await axios.get(prefix + "payment-info")).data,

  updateUserPaymentInfo: async (id: number, data: any) =>
    (await axios.put(prefix + "payment-info/" + id, data)).data,

  createUserPaymentInfo: async (data: any) =>
    (await axios.post(prefix + "payment-info", data)).data,

  deleteUserPaymentInfo: async (id: number) =>
    (await axios.delete(prefix + "payment-info/" + id)).data,

  queryMessageInbox: async (criteria?: any) =>
    (await axios.get(prefix + "message", { params: criteria })).data,

  sendOneTimeCodeForPhone: async (regionCode: string, phone: string) =>
    (await axios.post(`api/v1/user/otp/mobile/${regionCode}/${phone}`)).data,

  sendVerificationOneTimeCodeForPhone: async (
    regionCode: string,
    phone: string
  ) =>
    (await axios.post(`api/v1/user/verification/mobile/${regionCode}/${phone}`))
      .data,

  getConfiguration: async () =>
    (await axios.get("/api/configuration/public")).data,

  getMeIdentity: async () => (await axios.get("/api/v1/user/me")).data,

  registerNewUser: async (formData: any) =>
    (await axios.post("/api/v1/auth/register", formData)).data,

  resendConfirmationEmail: async (email: string, confirmUrl: string) =>
    (
      await axios.post("/api/v1/user/email/confirmation/resend", {
        email,
        confirmUrl,
      })
    ).data,

  verifyPhoneNumberChange: async (
    regionCode: string,
    phone: string,
    code: string
  ) =>
    (
      await axios.put(
        `api/v1/user/verification/mobile/${regionCode}/${phone}/${code}`
      )
    ).data,

  submitResetTradeAccountPassword: async (tenantId: any, formData: any) =>
    (
      await axios.put(
        "/api/v1/trade-account/" + tenantId + "/change-password",
        formData
      )
    ).data,

  getClientReferralCode: async () =>
    (await axios.get("/api/v1/user/me/refercode")).data,

  getReferralCodeSupplement: async (code: string) =>
    (await axios.get("api/v1/referralcode/" + code)).data,

  confirmEmail: async (formData: any) =>
    (await axios.post("/api/v1/user/email/confirm", formData)).data,

  createDemoAccountFromNonAuth: async (formData: any) =>
    (await axios.post("/api/v1/trade-demo-account", formData)).data,

  getCurrentIpInfo: async () =>
    (await axios.get("/api/v1/geoip/country/current")).data,

  getAllSymbols: async () => (await axios.get(prefix + "symbol")).data,

  getImagesWithGuid: async (guid: string) => {
    const response = await axios.get(`${prefix}media/${guid}`, {
      responseType: "blob",
    });
    const imageBlob = new Blob([response.data], { type: "image/jpeg" }); // Adjust the MIME type as necessary
    const imageBlobUrl = URL.createObjectURL(imageBlob);
    return imageBlobUrl;
  },

  queryAddressList: async (criteria?: any) =>
    (await axios.get(userPrefix + "address", { params: criteria })).data,

  deleteAddress: async (id: number) =>
    (await axios.delete(userPrefix + "address/" + id)).data,

  addAddress: async (formData: any) =>
    (await axios.post(userPrefix + "address", formData)).data,

  updateAddress: async (hashId: number, formData: any) =>
    (await axios.put(userPrefix + "address/" + hashId, formData)).data,

  getShopEvent: async () => (await axios.get(prefix + "event/shopEvent2")).data,

  getDepositGroups: async (uid: number) =>
    (
      await axios.get(
        prefixV2 + "payment-method/account/" + uid + "/deposit-group"
      )
    ).data,

  getAccountWithdrawGroups: async (uid: number) =>
    (
      await axios.get(
        prefixV2 + "payment-method/account/" + uid + "/withdrawal"
      )
    ).data,

  getWalletWithdrawGroups: async (hashid: any) =>
    (
      await axios.get(
        prefixV2 + "payment-method/wallet/" + hashid + "/withdrawal"
      )
    ).data,

  getAccountWithdrawGroupInfo: async (uid: number, hashId: any) =>
    (
      await axios.get(
        prefixV2 +
          "payment-method/" +
          hashId +
          "/account/" +
          uid +
          "/withdrawal-info"
      )
    ).data,

  getWalletWithdrawGroupInfo: async (walletHashId: any, hashId: any) =>
    (
      await axios.get(
        prefixV2 +
          "payment-method/" +
          hashId +
          "/wallet/" +
          walletHashId +
          "/withdrawal-info"
      )
    ).data,

  getDepositGroupInfo: async (uid: number, formData: any) =>
    (
      await axios.get(
        prefixV2 + "payment-method/account/" + uid + "/deposit-group-info",
        { params: formData }
      )
    ).data,

  postAccountDeposit: async (uid: number, formData: any) =>
    (await axios.post(prefixV2 + "account/" + uid + "/deposit", formData)).data,

  postPaypalCallback: async (formData: any) =>
    (await axios.post("api/v1/payment/callback/10000/paypal", formData)).data,

  getExLinkCurrencies: async () =>
    (await axios.get("api/v1/client/deposit/exlink/currencies")).data,

  getExLinkExchangeRates: async () =>
    (await axios.get("api/v1/client/deposit/exlink/exchange-rates")).data,
};

// Abcd1234!
