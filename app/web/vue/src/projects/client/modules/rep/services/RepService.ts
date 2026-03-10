import { axiosInstance as axios } from "@/core/services/api.client";
import { AccountGroupTypes } from "@/core/types/AccountGroupTypes";
import { computed } from "vue";
import store from "@/store";
import { normalizeAmountList } from "@/lib/utils";

export interface CreateLeadSpec {
  name: string;
  email: string;
  phoneNumber: string;
}

export interface AddedLeadCommentSpec {
  content: string;
}

const repAccount = computed(() => store.state.RepModule.repAccount);
const repPrefix = computed(() => `api/v1/rep/${repAccount.value?.uid}/`);

const repService = {
  queryAccounts: async (criteria?: any) =>
    (await axios.get(repPrefix.value + "account", { params: criteria })).data,

  queryRepLeads: async (criteria?: any) =>
    (await axios.get(repPrefix.value + "lead", { params: criteria })).data,

  queryTradeReportsOfRep: async (criteria?: any) =>
    (
      await axios.get(repPrefix.value + "tradetransaction", {
        params: criteria,
      })
    ).data,

  queryTransactionReportsOfRep: async (criteria?: any) => {
    const res = (
      await axios.get(repPrefix.value + "transaction", {
        params: criteria,
      })
    ).data;
    res.data = normalizeAmountList(res.data);
    return res;
  },

  queryRepDeposit: async (criteria?: any) => {
    const res = (
      await axios.get(repPrefix.value + "deposit", { params: criteria })
    ).data;
    res.data = normalizeAmountList(res.data);
    return res;
  },

  fuzzySearchAccount: async (criteria?: any) =>
    (
      await axios.get(repPrefix.value + "search/account", {
        params: criteria,
      })
    ).data,

  queryClientTransaction: async (accountUid: number, criteria?: any) =>
    (
      await axios.get(
        repPrefix.value + "trade-account/" + accountUid + "/transaction",
        { params: criteria }
      )
    ).data,

  queryClientTrade: async (accountUid: number, criteria?: any) =>
    (
      await axios.get(
        repPrefix.value + "trade-account/" + accountUid + "/trade",
        { params: criteria }
      )
    ).data,

  queryClientDeposit: async (criteria?: any) =>
    (await axios.get(repPrefix.value + "deposit", { params: criteria })).data,

  queryClientWithdrawal: async (criteria?: any) => {
    const res = (
      await axios.get(repPrefix.value + "withdrawal", { params: criteria })
    ).data;
    res.data = normalizeAmountList(res.data);
    return res;
  },

  getFullAccountGroupNames: async (type: AccountGroupTypes, keywords = "") =>
    (
      await axios.get(repPrefix.value + "account/group/name-list", {
        params: { type, keywords },
      })
    ).data,

  assignLeadToSalesAccount: async (
    leadId: number,
    assignedAccountUid: number
  ) =>
    (
      await axios.post(
        repPrefix.value + "lead/" + leadId + "/assign/" + assignedAccountUid
      )
    ).data,

  createLead: async (createLeadSpec: CreateLeadSpec) =>
    (await axios.post(repPrefix.value + "lead", createLeadSpec)).data,

  removeAssignedSalesAccount: async (
    leadId: number,
    assignedAccountUid: number
  ) =>
    (
      await axios.delete(
        repPrefix.value + "lead/" + leadId + "/assign/" + assignedAccountUid
      )
    ).data,

  getLeadDetails: async (leadId: number) =>
    (await axios.get(repPrefix.value + "lead/" + leadId)).data,

  addCommentToLead: async (
    leadId: number,
    addedLeadCommentSpec: AddedLeadCommentSpec
  ) =>
    (
      await axios.post(
        repPrefix.value + "lead/" + leadId + "/comment",
        addedLeadCommentSpec
      )
    ).data,

  archiveLead: async (leadId: number) =>
    (await axios.put(repPrefix.value + "lead/" + leadId + "/archive")).data,
};

export default repService;
