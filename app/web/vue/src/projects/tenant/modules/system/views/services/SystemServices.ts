import {
  axiosInstance as axios,
  paramsSerializerForDotnet,
} from "@/core/services/api.client";
import { AccountStatusTypes } from "@/core/types/AccountInfos";
import { AccountGroupTypes } from "@/core/types/AccountGroupTypes";

const prefix = "api/v1/tenant/";

export type AccountWizardType = {
  kycFormCompleted: boolean;
  paymentAccessGranted: boolean;
  welcomeEmailSent: boolean;
  noticeEmailSent: boolean;
};

const SystemServices = {
  queryAccounts: async (criteria?: any) =>
    (
      await axios.get(prefix + "account", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data,

  getAccountDetailById: async (id: number) =>
    (await axios.get(prefix + "account/" + id)).data,
};

export default SystemServices;
