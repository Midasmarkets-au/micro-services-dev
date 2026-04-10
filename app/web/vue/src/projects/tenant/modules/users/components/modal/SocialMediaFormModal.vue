<template>
  <el-dialog
    v-model="socialMediaRef"
    title="Social Media"
    width="800"
    align-center
    class="rounded"
    :before-close="hide"
  >
    <el-form ref="ruleFormRef" :model="item" label-position="top">
      <div class="row">
        <div class="col-6">
          <el-form-item
            v-if="formType == 'edit'"
            label="App Name"
            prop="appName"
          >
            <el-input :value="$t('fields.' + item.name)" disabled></el-input>
          </el-form-item>

          <el-form-item
            v-if="formType == 'create'"
            label="App Name ( Contact IT to add more app )"
            prop="appName"
          >
            <el-select v-model="item.name">
              <el-option
                v-for="item in socialMediaOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item label="Account ID" prop="accountId">
            <el-input v-model="item.account"></el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-6">
          <el-form-item label="MDM Connect ID" prop="BCRConnectId">
            <el-input v-model="item.connectId"></el-input>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item label="Staff Name" prop="staffName">
            <el-input v-model="item.staffName"></el-input>
          </el-form-item>
        </div>
      </div>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="socialMediaRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="update" :loading="isLoading">
          {{ $t("action.update") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import { ApplicationStatusType } from "@/core/types/ApplicationInfos";

const isLoading = ref(false);
const socialMediaRef = ref(false);
const partyId = ref(0);
const formType = ref("");
const item = ref({
  name: "",
  account: "",
  connectId: "",
  staffName: "",
});

const socialMediaOptions = [
  { label: "Facebook", value: "facebook" },
  { label: "Twitter", value: "twitter" },
  { label: "Instagram", value: "instagram" },
  { label: "LinkedIn", value: "linkedIn" },
  { label: "WeChat", value: "weChat" },
  { label: "WhatsApp", value: "whatsApp" },
  { label: "Telegram", value: "telegram" },
  { label: "Line", value: "line" },
];

const emits = defineEmits<{
  (e: "application-submitted"): void;
}>();

const update = async () => {
  isLoading.value = true;
  try {
    await UserService.putSocialMediaInfo(partyId.value, item.value);
    emits("application-submitted");
    hide();
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const show = (_partyId, _type, _item?) => {
  partyId.value = _partyId;
  formType.value = _type;
  if (_type === "edit") item.value = JSON.parse(JSON.stringify(_item));
  socialMediaRef.value = true;
};

const hide = () => {
  socialMediaRef.value = false;
  formType.value = "";
  item.value = {
    name: "",
    account: "",
    connectId: "",
    staffName: "",
  };
};

defineExpose({
  show,
});
</script>
