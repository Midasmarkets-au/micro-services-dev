import {
  axiosInstance as axios,
  axiosInstance2 as axiosV2,
  processKeysToCamelCase,
} from "@/core/services/api.client";

import { AccountInfoCriteria } from "../types/Criterias";
import { VerificationDocumentTypes } from "@/core/types/VerificationInfos";
import { ApplicationApproveSupplement } from "@/core/models/Application";
import { normalizeAmountList } from "@/lib/utils";
import Can from "@/core/plugins/ICan";
import Clipboard from "clipboard";
const prefix = "api/v1/tenant/";
const prefixV2 = "v2/tenant/";
export interface CreateLeadSpec {
  name: string;
  email: string;
  phoneNumber: string;
}

export interface AddedLeadCommentSpec {
  content: string;
}

export default {
  sendEmailToUser: async (formData: any) =>
    (await axios.post(prefix + "email/send-to-user", formData)).data,

  // user roles api
  enableLeadAutoAssign: async () =>
    (await axios.put(prefix + "lead/auto-assign/enable")).data,

  disableLeadAutoAssign: async () =>
    (await axios.put(prefix + "lead/auto-assign/disable")).data,

  changeLeadAutoAssignUid: async (formData: any) =>
    (await axios.put(prefix + "lead/auto-assign/set-assignee", formData)).data,

  getLeadsAutoAssignSetting: async () =>
    (await axios.get(prefix + "lead/auto-assign/info")).data,

  removeAssignedSalesAccount: async (
    leadId: number,
    assignedAccountUid: number
  ) =>
    (
      await axios.delete(
        prefix + "lead/" + leadId + "/assign/" + assignedAccountUid
      )
    ).data,

  addCommentToLead: async (
    leadId: number,
    addedLeadCommentSpec: AddedLeadCommentSpec
  ) =>
    (
      await axios.post(
        prefix + "lead/" + leadId + "/comment",
        addedLeadCommentSpec
      )
    ).data,

  archiveLead: async (leadId: number) =>
    (await axios.put(prefix + "lead/" + leadId + "/archive")).data,

  getLeadDetails: async (leadId: number) =>
    (await axios.get(prefix + "lead/" + leadId)).data,

  assignLeadToSalesAccount: async (
    leadId: number,
    assignedAccountUid: number
  ) =>
    (
      await axios.post(
        prefix + "lead/" + leadId + "/assign/" + assignedAccountUid
      )
    ).data,

  createLead: async (createLeadSpec: CreateLeadSpec) =>
    (await axios.post(prefix + "lead", createLeadSpec)).data,

  queryRepLeads: async (criteria?: any) =>
    (await axios.get(prefix + "lead", { params: criteria })).data,

  generateVerificationCode: async (partyId: number) =>
    (await axios.post(prefix + "user/" + partyId + "/verification-code")).data,

  migrateUnverifiedUser: async (partyId: number, tenantId: number) =>
    (await axios.post(prefix + "user/" + partyId + "/migrate-to/" + tenantId))
      .data,

  updateUserTag: async (partyId: number, tag: string) =>
    (await axios.put(prefix + "user/" + partyId + "/tag/" + tag)).data,

  deleteUserTag: async (partyId: number, tag: string) =>
    (await axios.delete(prefix + "user/" + partyId + "/tag/" + tag)).data,

  wsPopupTest: async () =>
    (await axios.get(prefix + "super-admin/test-popup-ws")).data,

  queryUserRoles: async () => (await axios.get(prefix + "user/role")).data,

  addUserRole: async (partyId: number, roleId: number) =>
    (await axios.put(prefix + "user/" + partyId + "/role/" + roleId + "/add"))
      .data,

  removeUserRole: async (partyId: number, roleId: number) =>
    (
      await axios.put(
        prefix + "user/" + partyId + "/role/" + roleId + "/remove"
      )
    ).data,

  // all others
  getUsers: async (criteria?: any) =>
    (await axios.get(prefix + "user", { params: criteria })).data,

  putSocialMediaInfo: async (partyId: number, formData: any) =>
    (
      await axios.put(
        prefix + "user/" + partyId + "/social-media-info",
        formData
      )
    ).data,

  getSocialMediaInfo: async (partyId: number) =>
    (await axios.get(prefix + "user/" + partyId + "/social-media-info/")).data,

  deleteDemoAccount: async (id: number) =>
    (await axios.delete(prefix + "trade-demo-account/" + id)).data,

  queryDemoAccounts: async (criteria?: any) =>
    (await axios.get(prefix + "trade-demo-account", { params: criteria })).data,

  openTradeAccount: async (partyId: number, formData: any) =>
    (
      await axios.post(
        prefix + "application/for-user/" + partyId + "/trade-account",
        formData
      )
    ).data,
  switchTenant: async (
    partyId: number,
    sourceTenantId: number,
    targetTenantId: number
  ) =>
    (
      await axios.post(
        prefix +
          "user/" +
          partyId +
          "/migrate/" +
          sourceTenantId +
          "/tenant/" +
          targetTenantId
      )
    ).data,
  fetchAllUsers: async (criteria?: any) =>
    (await axios.get(prefix + "user", { params: criteria })).data,

  switchQuizLock: async (partyId: number) =>
    (await axios.put(prefix + "user/" + partyId + "/unlock/quiz")).data,

  updateUserStatus: async (partyId: number, formData: any) =>
    (await axios.put(prefix + "user/" + partyId + "/status", formData)).data,

  getClientMediaList: async (criteria?: any) =>
    (await axios.get(prefix + "media/list", { params: criteria })).data,

  getWalletTransactionByWalletId: async (walletId: number, criteria?: any) => {
    const res = (
      await axios.get(prefix + "wallet/" + walletId + "/transaction", {
        params: criteria,
      })
    ).data;
    const re = {
      ...res,
      data: res.data
        ? normalizeAmountList(res.data, ["prevBalance", "amount"])
        : res.data,
    };
    console.log("re", re);
    return re;
  },

  putUserToDelayReview: async (id: number) =>
    (await axios.put(prefix + "verification/" + id + "/delayed-approve")).data,

  updateSiteID: async (partyId: number, siteId: number) =>
    (await axios.put(prefix + "user/" + partyId + "/site/" + siteId)).data,

  updateAccountSiteID: async (accountId: number, formData: any) =>
    (await axios.put(prefix + "account/" + accountId + "/site", formData)).data,

  updateAccountType: async (accountId: number, formData: any) =>
    (await axios.put(prefix + "account/" + accountId + "/type", formData)).data,

  getComplianceSignature: async () =>
    (await axios.get(prefix + "kyc/complianceSig")).data,

  queryUsers: async (criteria?: any) =>
    (await axios.get(prefix + "user", { params: criteria })).data,

  getServices: async () => (await axios.get(prefix + "trade/service")).data,

  getServiceById: async (id: number) =>
    (await axios.get(prefix + "trade/service/" + id + "/group")).data,

  deleteGroupById: async (id: number, formData: any) =>
    (
      await axios.delete(prefix + "trade/service/" + id + "/group", {
        data: formData,
      })
    ).data,

  addGroupById: async (id: number, formData: any) =>
    (await axios.post(prefix + "trade/service/" + id + "/group", formData))
      .data,

  updateGroupById: async (id: number, formData: any) =>
    (await axios.put(prefix + "trade/service/group/" + id, formData)).data,

  queryApplications: async (criteria?: any) =>
    (await axios.get(prefix + "application", { params: criteria })).data,

  queryUserAccounts: async (criteria?: AccountInfoCriteria) =>
    (await axios.get(prefix + "account", { params: criteria })).data,

  approveApplication: async (
    applicationId: number,
    formData: ApplicationApproveSupplement
  ) =>
    (
      await axios.put(
        prefix + "application/" + applicationId + "/approve",
        formData
      )
    ).data,

  completeApplication: async (applicationId: number, formData: any) =>
    (
      await axios.put(
        prefix + "application/" + applicationId + "/complete",
        formData
      )
    ).data,

  createTradeAccount: async (formData: any) =>
    (await axios.post(prefix + "trade-account", formData)).data,

  postEventTopic: async (formData: any) =>
    (await axios.post(prefix + "topic", formData)).data,

  getVerificationById: async (id: number) =>
    (await axios.get(prefix + "verification/" + id)).data,

  getOldPersonalInfo: async (partyId: number) =>
    (await axios.get(prefix + partyId + "/legacy/personal-info")).data,

  getOldFinancialInfo: async (partyId: number) =>
    (await axios.get(prefix + partyId + "/legacy/financial-info")).data,

  getUserInfoByPartyId: async (partyId: number) =>
    (await axios.get(prefix + "user/" + partyId)).data,

  queryKycForms: async (criteria?: any) =>
    (await axios.get(prefix + "kyc", { params: criteria })).data,

  createKycForm: async (partyId: number, formData: any) =>
    (await axios.post(prefix + "kyc/" + partyId, formData)).data,

  putKycForm: async (partyId: number, formData: any) =>
    (await axios.put(prefix + "kyc/" + partyId, formData)).data,

  rejectKycForm: async (partyId: number) =>
    (await axios.put(prefix + "kyc/" + partyId + "/reject")).data,

  getKycForm: async (partyId: any) => {
    let res: any;
    try {
      res = (await axios.get(prefix + "kyc/" + partyId)).data;
    } catch (err) {
      res = {};
    }
    return res.verificationItems?.length
      ? res.verificationItems[0].data
      : { emptyKycForm: true };
  },

  getLegacyUserPersonalInfoByPartyId: async (partyId: number) => {
    const res = (
      await axios.get(prefix + "user/" + partyId + "/legacy/personal-info")
    ).data;
    return res.length === 0 ? {} : processKeysToCamelCase(res[0]);
  },

  getLegacyUserFinancialInfoByPartyId: async (partyId: number) => {
    const res = (
      await axios.get(prefix + "user/" + partyId + "/legacy/financial-info")
    ).data;
    return res.length === 0 ? {} : processKeysToCamelCase(res[0]);
  },

  getPredefinedKycInfos: async function (partyId: number) {
    const socialMediaInfos = await this.getSocialMediaInfo(partyId);
    const userInfo = await this.getUserInfoByPartyId(partyId);
    const kycForm = await this.getKycForm(partyId);
    const legacyPersonalInfo = await this.getLegacyUserPersonalInfoByPartyId(
      partyId
    );
    const legacyFinancialInfo = await this.getLegacyUserFinancialInfoByPartyId(
      partyId
    );
    const userVerification = await this.queryVerifications({ partyId });
    let financialInfo = {} as any,
      personalInfo = {} as any;
    if (userVerification.data.length > 0) {
      const verificationDetails = await this.getVerificationById(
        userVerification.data[0].id
      );
      financialInfo = verificationDetails.financial ?? {};
      personalInfo = verificationDetails.info ?? {};
    }
    // console.log("userInfo", userInfo);
    // console.log("kycForm", kycForm);
    // console.log("legacyPersonalInfo", legacyPersonalInfo);
    // console.log("legacyFinancialInfo", legacyFinancialInfo);
    // console.log("============================================");
    return {
      ...kycForm,
      accountNumber: kycForm.accountNumber,
      socialMediaTypes:
        socialMediaInfos ||
        kycForm.socialMediaTypes ||
        personalInfo.socialMediaTypes,

      firstName:
        kycForm.firstName ||
        userInfo.firstName ||
        personalInfo.firstName ||
        legacyPersonalInfo.firstName,

      lastName:
        kycForm.lastName ||
        userInfo.lastName ||
        personalInfo.lastName ||
        legacyPersonalInfo.lastName,

      priorName:
        userInfo.priorName ||
        personalInfo.priorName ||
        legacyPersonalInfo.priorName,

      birthday:
        kycForm.birthday ||
        userInfo.birthday ||
        personalInfo.birthday ||
        legacyPersonalInfo.dateOfBirth,

      address:
        kycForm.address ||
        userInfo.address ||
        personalInfo.address ||
        legacyPersonalInfo.address,

      citizen:
        kycForm.citizen ||
        userInfo.citizen ||
        personalInfo.citizen ||
        legacyPersonalInfo.citizenOf,

      email:
        kycForm.email ||
        userInfo.email ||
        personalInfo.email ||
        legacyPersonalInfo.email,

      annualIncome:
        kycForm.annualIncome ||
        financialInfo.income ||
        {
          ["大于 $450,000"]: "1",
          ["$200,000 至 $449,999"]: "2",
          ["$90,000 至 $199,999"]: "3",
          ["$60,000 至 $89,999"]: "4",
          ["$15,000 至 $59,999"]: "5",
          ["小于 $15,000"]: "6",
        }[legacyFinancialInfo.income] ||
        "6",

      netWorth:
        kycForm.netWorth ||
        financialInfo.investment ||
        {
          ["大于 $450,000"]: "1",
          ["$200,000 至 $449,999"]: "2",
          ["$90,000 至 $199,999"]: "3",
          ["$60,000 至 $89,999"]: "4",
          ["$15,000 至 $59,999"]: "5",
          ["小于 $15,000"]: "6",
        }[legacyFinancialInfo.property] ||
        "6",

      idType: parseInt(
        kycForm.idType || userInfo.idType || legacyPersonalInfo.formOfId
      ),

      idIssuer:
        kycForm.idIssuer ||
        userInfo.idIssuer ||
        legacyPersonalInfo.officeOfIssue,

      idExpireOn:
        kycForm.idExpireOn ||
        userInfo.idExpireOn ||
        legacyPersonalInfo.expiryDate,

      idIssuedOn:
        kycForm.idIssueOn ||
        userInfo.idIssuedOn ||
        legacyPersonalInfo.issuedDate,

      idNumber:
        kycForm.idNumber || userInfo.idNumber || legacyPersonalInfo.idNumber,

      industry: financialInfo.industry,
      occupation: financialInfo.position,
      sourceOfFunds: financialInfo.fund
        ?.filter((item) => item !== "other")
        .join(", "),
      bg1: financialInfo.bg1,
      bg2: financialInfo.bg2,
      exp1: financialInfo.exp1,
      exp1Employer: financialInfo.exp1_employer,
      exp1Position: financialInfo.exp1_position,
      exp1Remuneration: financialInfo.exp1_remuneration,
      exp2: financialInfo.exp2,
      exp2More: financialInfo.exp2_more,
      exp3: financialInfo.exp3,
      exp3More: financialInfo.exp3_more,
      exp4: financialInfo.exp4,
      exp4More: financialInfo.exp4_more,
      exp5: financialInfo.exp5,
      exp5More: financialInfo.exp5_more,

      legacyFinancialInfo,
      // ...kycForm,
    };
  },

  signKycForm: async (partyId: number, formData: any) =>
    (await axios.put(prefix + "kyc/" + partyId + "/sign", formData)).data,

  finalizeKycForm: async (partyId: number, formData: any) =>
    (await axios.put(prefix + "kyc/" + partyId + "/finalize", formData)).data,

  queryVerifications: async (criteria?: any) =>
    (await axios.get(prefix + "verification", { params: criteria })).data,

  queryLeads: async (criteria?: any) =>
    (await axios.get(prefix + "lead", { params: criteria })).data,

  assignLeads2SalesAccount: async (formData: any) =>
    (await axios.put(prefix + "lead/assign", formData)).data,

  putUserToUnderReview: async (id: number) =>
    (await axios.put(prefix + "verification/" + id + "/under-review")).data,

  putUserToApproved: async (id: number) =>
    (await axios.put(prefix + "verification/" + id + "/approve")).data,

  putUserToAwaitingReview: async (id: number) =>
    (await axios.put(prefix + "verification/" + id + "/awaiting-review")).data,

  putUserToAwaitingApprove: async (id: number) =>
    (await axios.put(prefix + "verification/" + id + "/awaiting-approve")).data,

  putUserToAwaitingVerify: async (id: number) =>
    (
      await axios.put(
        prefix + "verification/" + id + "/awaiting-address-verify"
      )
    ).data,

  putUserToAwaitingCodeVerify: async (id: number) =>
    (await axios.put(prefix + "verification/" + id + "/awaiting-code-verify"))
      .data,

  generateMailCode: async (id: number) =>
    (await axios.post(prefix + "verification/" + id + "/mail-code")).data,
  getMailCode: async (id: number) =>
    (await axios.get(prefix + "verification/" + id + "/mail-code")).data,

  // not fully functional: 08/01/2023
  putUserToRejected: async (id: number) =>
    (await axios.put(prefix + "verification/" + id + "/reject")).data,

  approveUserDocument: async (
    verificationId: number,
    mediumId: number,
    formData?: any
  ) =>
    (
      await axios.put(
        prefix + `verification/${verificationId}/document/${mediumId}/approve`,
        formData
      )
    ).data,

  rejectUserDocument: async (
    verificationId: number,
    mediumId: number,
    formData?: any
  ) =>
    (
      await axios.put(
        prefix + `verification/${verificationId}/document/${mediumId}/reject`,
        formData
      )
    ).data,

  deleteVerificationDocForUser: async (
    verificationId: number,
    mediumId: number
  ) =>
    (
      await axios.delete(
        prefix + "verification/" + verificationId + "/document/" + mediumId
      )
    ).data,

  uploadVerificationDocForUser: async (
    verificationId: number,
    type: VerificationDocumentTypes,
    fileForm: any
  ) =>
    (
      await axios.post(
        prefix + "verification/" + verificationId + "/document/upload",
        fileForm,
        { params: { type } }
      )
    ).data,

  sendRejectDocumentEmail: async (verificationId: number) =>
    (
      await axios.put(
        prefix + "verification/" + verificationId + "/document/reject-notice"
      )
    ).data,

  requestUserToken: async (partyId: number) =>
    (await axios.post(prefix + "user/" + partyId + "/god-mode")).data,

  requestUnlockUser: async (partyId: number) =>
    (await axios.put(prefix + "user/" + partyId + "/unlock")).data,

  requestLockUser: async (partyId: number) =>
    (await axios.put(prefix + "user/" + partyId + "/lock")).data,

  fuzzySearchUsers: async (fuzzyKey: string, currentPage = 1, pageSize = 10) =>
    (
      await axios.post(prefix + "search/user", {
        q: fuzzyKey,
        page: currentPage,
        pageSize,
        sort: ["createdOn:desc"],
      })
    ).data,

  rebuildUserIndex: async (partyId: number) =>
    (await axios.post(prefix + "search/user/rebuild", { partyId: partyId }))
      .data,

  generateGodModeHandler: () => {
    let clickCount = 0;
    let clickTimeout: any = null;

    return (_partyId: number) => {
      if (!Can.cans(["SuperAdmin"])) return;
      clickCount++;
      if (clickTimeout) {
        clearTimeout(clickTimeout);
      }

      clickTimeout = setTimeout(async () => {
        if (clickCount >= 3) {
          const res = (
            await axios.post(prefix + "user/" + _partyId + "/god-mode")
          ).data;
          Clipboard.copy(
            (process.env.VUE_APP_URL +
              "/set-token?token=" +
              res.token) as string
          );
          window.open(
            process.env.VUE_APP_URL + "/set-token?token=" + res.token,
            "_blank"
          );
          clickCount = 0;
        }
        clickCount = 0;
      }, 500);
    };
  },
};
