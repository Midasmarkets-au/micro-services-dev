<template>
  <div>
    <div class="card">
      <div class="card-header">
        <h3 class="card-title">
          <!--          {{ $t("title.readOnlyPassLetter") }}-->
          Read Only Password Letter
        </h3>
        <div class="card-toolbar">
          <span class="me-3 fw-semibold">{{ $t("action.attempted") }}</span>
          <span
            class="bg-primary text-white w-25px h-25px rounded d-flex justify-content-center align-items-center fw-semibold"
            >{{ 0 }}</span
          >
        </div>
      </div>

      <div class="card-body">
        <el-form
          ref="ruleFormRef"
          :model="ruleForm"
          :rules="rules"
          label-width="150px"
          class="demo-ruleForm"
          size="default"
          status-icon
        >
          <el-form-item label="Account" prop="account">
            <el-input v-model="ruleForm.account" disabled />
          </el-form-item>

          <el-form-item label="Name" prop="name">
            <el-input v-model="ruleForm.name" disabled />
          </el-form-item>

          <el-form-item label="User Email" prop="email">
            <el-input v-model="ruleForm.email" disabled />
          </el-form-item>

          <el-form-item label="Language" prop="language">
            <el-radio-group v-model="ruleForm.language">
              <el-radio
                v-for="item in languageSelections"
                :key="item.code"
                :label="item.code"
                >{{ item.name }}</el-radio
              >
            </el-radio-group>
          </el-form-item>

          <el-form-item label="Send To" prop="bccs">
            <el-checkbox
              v-model="sendToSales"
              v-if="accountDetails.salesAccount?.uid !== 0"
            >
              Sales: {{ accountDetails.salesAccount?.user?.email }}
            </el-checkbox>

            <el-checkbox
              v-model="sendToIb"
              v-if="accountDetails.agentAccount?.uid !== 0"
            >
              IB: {{ accountDetails.agentAccount?.user?.email }}
            </el-checkbox>
          </el-form-item>

          <el-form-item label="Read Only" prop="readOnlyPassword">
            <el-input v-model="ruleForm.readOnlyPassword" />
          </el-form-item>

          <el-form-item label="Date" prop="date">
            <el-date-picker
              v-model="ruleForm.date"
              type="date"
              label="Pick a date"
              :placeholder="$t('action.select')"
              style="width: 100%"
            />
          </el-form-item>

          <el-form-item label="Other Emails" prop="otherEmails">
            <el-input
              v-model="ruleForm.otherEmails"
              placeholder="use ,  to separate"
            />
          </el-form-item>

          <el-form-item label="" prop="sendOutIndividually">
            <el-checkbox
              v-model="ruleForm.sendOutIndividually"
              label="Send out individually"
            />
          </el-form-item>

          <el-form-item>
            <el-button type="primary" @click="submitForm(ruleFormRef)"
              >Submit</el-button
            >
            <el-button @click="previewMailHtml">Preview</el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>

    <div class="card mt-5">
      <div class="card-header">
        <div class="card-title">{{ $t("title.history") }}</div>
        <div class="card-toolbar"></div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">id</th>
              <th class="">{{ $t("fields.operator") + "Id" }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && kycHistories.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in kycHistories" :key="index">
              <td>{{ item.id }}</td>
              <td>{{ item.partyId }}</td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="getKycSendHistory" :criteria="criteria" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, inject, Ref, computed } from "vue";
import type { FormInstance, FormRules } from "element-plus";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import InjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
// import MsgPrompt from "@/core/plugins/MsgPrompt";
import TimeShow from "@/components/TimeShow.vue";
import { ElNotification } from "element-plus";

const emits = defineEmits<{
  (e: "submit"): void;
}>();

const languageSelections = ref(LanguageTypes.all);

const getUserInfos = inject<() => object>(
  AccountInjectionKeys.GET_USER_INFOS,
  () => ({})
);
const userInfos = ref<any>({});

const accountDetails = inject<Ref>(
  AccountInjectionKeys.ACCOUNT_DETAILS,
  ref<any>({})
);
const getReadOnlyCode = inject<() => Promise<string>>(
  AccountInjectionKeys.GET_READ_ONLY_CODE,
  () => Promise.resolve("")
);

const openFilePreviewModal = inject(InjectionKeys.OPEN_FILE_MODAL, (media) =>
  // MsgPrompt.error("Not implemented" + media)
  ElNotification({
    title: "Error",
    message: "Not implemented" + media,
    type: "warning",
  })
);

const ruleFormRef = ref<FormInstance>();
const ruleForm = ref<any>({});
const sendToSales = ref(accountDetails.value.salesAccount?.user?.email !== "");
const sendToIb = ref(accountDetails.value.agentAccount?.user?.email !== "");

const kycHistories = ref<any>([]);
const isLoading = ref(false);
const criteria = ref<any>({
  page: 1,
  size: 5,
});

const rules = reactive<FormRules>({
  account: [
    {
      required: true,
      message: "Please input  account",
      trigger: "blur",
    },
  ],

  name: [
    {
      required: true,
      message: "Please input name",
      trigger: "blur",
    },
  ],

  email: [
    {
      required: true,
      message: "Please input email",
      trigger: "blur",
    },
  ],

  language: [
    {
      required: true,
      message: "Please select language",
      trigger: "change",
    },
  ],

  readOnlyPassword: [
    {
      required: true,
      message: "Please input read only password",
      trigger: "blur",
    },
  ],

  date: [
    {
      type: "date",
      required: true,
      message: "Please pick a date",
      trigger: "change",
    },
  ],
});

/**
 * [
 *   sendToSales.value && accountDetails.value.salesAccount.user.email !== ""
 *     ? accountDetails.value.salesAccount.user.email
 *     : undefined,
 *   sendToIb.value && accountDetails.value.agentAccount.user.email !== ""
 *     ? accountDetails.value.agentAccount.user.email
 *     : undefined,
 * ]
 */
const emailList = computed(() => {
  const agentEmail = accountDetails.value.agentAccount.user.email;
  const salesEmail = accountDetails.value.salesAccount.user.email;
  if (
    (!sendToSales.value && !sendToIb.value) ||
    (agentEmail !== "" && salesEmail !== "")
  ) {
    return [];
  }

  if (sendToIb.value && agentEmail !== "") {
    return [agentEmail];
  }

  if (sendToSales.value && salesEmail !== "") {
    return [salesEmail];
  }

  return [agentEmail, salesEmail];
});

const previewMailHtml = async () => {
  const res = await AccountService.getReadOnlyCodeMailReview(
    accountDetails.value.id,
    {
      to: userInfos.value.email,
      bcc: emailList.value,
    }
  );
  openFilePreviewModal({
    content: res,
    title: "Preview",
    contentType: "text/html",
  });
};

const submitForm = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid) => {
    if (!valid) return;

    const bccEmails = ruleForm.value.otherEmails
      .split(",")
      .map((item) => item.trim());

    // if individually or no ib or sales emails
    if (ruleForm.value.sendOutIndividually || !emailList.value.length) {
      [...bccEmails, ...emailList.value].forEach((email) =>
        AccountService.postReadOnlyCodeMail(accountDetails.value.id, {
          to: email,
        })
      );
    } else {
      // sales or ib need to send out, and other emails also need to send out
      // mark other emails as bcc
      [...emailList.value].forEach((email) =>
        AccountService.postReadOnlyCodeMail(accountDetails.value.id, {
          to: email,
          bccs: bccEmails,
        })
      );
    }
    // MsgPrompt.success("Send successfully").then(() => {
    //   emits("submit");
    // });
    ElNotification({
      title: "Success",
      message: "Read Only Notice Send successfully",
      type: "success",
    });
    emits("submit");
  });
};

const resetForm = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  userInfos.value = await getUserInfos();
  const readOnlyCode = await getReadOnlyCode();
  ruleForm.value = {
    account: accountDetails.value.tradeAccount.accountNumber,
    name: `${userInfos.value.firstName} ${userInfos.value.lastName}`,
    email: userInfos.value.email,
    language: userInfos.value.language,
    bccs: false,
    readOnlyPassword: readOnlyCode,
    date: new Date(),
    otherEmails: "",
    sendOutIndividually: false,
  };
  formEl.resetFields();
};

const getKycSendHistory = async (_page: number) => {
  try {
    isLoading.value = true;
    criteria.value.page = _page;
    const res = await AccountService.queryCommunications({
      rowId: accountDetails.value.id,
      type: 100,
    });
    kycHistories.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  resetForm(ruleFormRef.value);
  getKycSendHistory(1);
});
</script>

<style scoped></style>
