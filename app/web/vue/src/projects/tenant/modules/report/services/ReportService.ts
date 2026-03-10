import { axiosInstance as axios } from "@/core/services/api.client";
import { axiosInstance2 as axios2 } from "@/core/services/api.client";
import { axiosInstance2 as axiosV2 } from "@/core/services/api.client";

const prefix = "api/v1/tenant/";
const v2Prefix = "v2/tenant/report";

const ReportService = {
  getUserDataByUid: async (uid: number) =>
    (await axios.get(prefix + "user/uid/" + uid)).data,

  postReportRequest: async (formData: any) =>
    (await axiosV2.post(v2Prefix, formData)).data,
  createReportType: async (formData: any) =>
    (await axiosV2.post(v2Prefix + "/report-type", formData)).data,

  queryComments: async (criteria?: any) =>
    (await axios.get(prefix + "comment", { params: criteria })).data,

  createSalesReport: async (criteria?: any) =>
    (await axiosV2.get(v2Prefix + "/sales", { params: criteria })).data,

  createIbReport: async (criteria?: any) =>
    (await axiosV2.get(v2Prefix + "/ib/tenant-report", { params: criteria }))
      .data,

  queryMessageRecordById: async (id: number) =>
    (await axios.get(prefix + "message-record/" + id)).data,
  queryMessageRecords: async (criteria?: any) =>
    (await axios.get(prefix + "message-record", { params: criteria })).data,
  queryRequests: async (criteria?: any) =>
    (await axios.get(prefix + "report/request", { params: criteria })).data,

  // Client Confirmation
  queryAccountReport: async (criteria?: any) =>
    (await axios.get(prefix + "account-report", { params: criteria })).data,

  sendAccountReport: async (id: number, formData?: any) =>
    (
      await axios.post(
        prefix + "account-report/" + id + "/send-report",
        formData
      )
    ).data,

  queryAccountReportPreviewById: async (id: number) =>
    (await axios.get(prefix + "account-report/" + id + "/preview")).data,

  queryReqortTypes: async () =>
    (await axios2.get(v2Prefix + "/types", {})).data,

  queryGroups: async () =>
    (await axios2.get(v2Prefix + "/account-groups", {})).data,

  //Equity Report
  queryEquityReportTypes: async () =>
    (await axios2.get(v2Prefix + "/equity-report-types")).data,
  queryEquityReport: async (criteria?: any) =>
    (await axios2.get(v2Prefix + "/equity-report", { params: criteria })).data,
  queryEquityAccountInfo: async (accountNumber: number) =>
    (await axios2.get(v2Prefix + "/equity-account-info/" + accountNumber)).data,

  // 重新生成报告
  regenerateReport: async (id: number) =>
    (await axios.post(prefix + "report/request/" + id + "/regen")).data,
};

export default ReportService;
