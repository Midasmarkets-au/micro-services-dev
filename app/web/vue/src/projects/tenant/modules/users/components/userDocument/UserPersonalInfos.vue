<template>
  <div>
    <el-form
      label-width="auto"
      class="demo-ruleForm"
      size="default"
      status-icon
    >
      <el-form-item label="Last Login IP">
        <el-input v-model="accountDetails.lastLoginIp" disabled></el-input>
      </el-form-item>
      <el-form-item label="Registered IP">
        <el-input v-model="accountDetails.registeredIp" disabled></el-input>
      </el-form-item>

      <div v-if="formData.socialMedium">
        <div v-for="(type, index) in formData.socialMedium" :key="index">
          <div v-if="type.account != ''" class="d-flex justify-content-between">
            <el-form-item
              :label="$t('fields.' + type.name)"
              :prop="type.name"
              style="width: 90%"
            >
              <el-input v-model="type.account" disabled></el-input>
            </el-form-item>

            <CopyButton class="ms-3" :content="type.account" />
          </div>
        </div>
        <hr class="mb-7" />
      </div>
      <div class="d-flex justify-content-between">
        <el-form-item label="Site ID" prop="siteID" style="width: 90%">
          <el-input v-model="showSiteId" disabled></el-input>
        </el-form-item>

        <el-button
          class="ms-3"
          type="primary"
          @click="changeSiteModalRef?.show(partyId, siteId)"
          >Update</el-button
        >
      </div>

      <el-form-item label="Status">
        <el-switch
          v-model="accountDetails.status"
          active-color="#13ce66"
          inactive-color="#ff4949"
          active-text="Active"
          inactive-text="Inactive"
          :active-value="PartyStatusTypes.Active"
          :inactive-value="PartyStatusTypes.Closed"
          @click="updateStatus(accountDetails.status)"
        />
      </el-form-item>
    </el-form>

    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-width="auto"
      class="demo-ruleForm"
      size="default"
      status-icon
    >
      <el-form-item label="Email" prop="email">
        <el-input v-model="formData.email"></el-input>
      </el-form-item>
      <el-form-item label="Native Name" prop="nativeName">
        <el-input v-model="formData.nativeName"></el-input>
      </el-form-item>

      <el-form-item label="First Name" prop="firstName">
        <el-input v-model="formData.firstName"></el-input>
      </el-form-item>

      <el-form-item label="Last Name" prop="lastName">
        <el-input v-model="formData.lastName"></el-input>
      </el-form-item>
      <el-form-item label="Birthday" prop="birthday">
        <el-date-picker
          v-model="formData.birthday"
          type="date"
          value-format="YYYY-MM-DD"
        ></el-date-picker>
      </el-form-item>
      <el-form-item label="Gender" prop="gender">
        <el-radio-group v-model="formData.gender">
          <el-radio :label="0"> Male</el-radio>
          <el-radio :label="1"> Female</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="Citizen" prop="citizen">
        <el-input v-model="formData.citizen"></el-input>
      </el-form-item>

      <el-form-item label="Address" prop="address">
        <el-input v-model="formData.address"></el-input>
      </el-form-item>

      <el-form-item label="CCC" prop="ccc">
        <el-input v-model="formData.ccc"></el-input>
      </el-form-item>

      <el-form-item label="Phone" prop="phoneNumber">
        <el-input v-model="formData.phone"></el-input>
      </el-form-item>

      <el-form-item label="ID Type" prop="idType">
        <el-radio-group v-model="formData.idType">
          <el-radio :label="1"> {{ $t("fields.govId") }}</el-radio>
          <el-radio :label="2"> {{ $t("fields.driveLicense") }}</el-radio>
          <el-radio :label="3"> {{ $t("fields.passport") }}</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="ID Number" prop="idNumber">
        <el-input v-model="formData.idNumber"></el-input>
      </el-form-item>

      <el-form-item label="ID Issuer" prop="idIssuer">
        <el-input v-model="formData.idIssuer"></el-input>
      </el-form-item>

      <el-form-item label="ID Issued On" prop="idIssuedOn">
        <el-date-picker
          v-model="formData.idIssuedOn"
          type="date"
          value-format="YYYY-MM-DD"
        ></el-date-picker>
      </el-form-item>

      <el-form-item :label="$t('fields.expire_date')" prop="idExpireOn">
        <el-date-picker
          v-model="formData.idExpireOn"
          type="date"
          value-format="YYYY-MM-DD"
        ></el-date-picker>
      </el-form-item>
      <!-- <el-form-item label="Roles" prop="roles">
        <UserPermissions :userDetails="props.userDetails" />
      </el-form-item> -->
      <el-form-item :label-width="100" label="Refer Code" prop="Refer Code">
        <el-input v-model="formData.referCode"></el-input>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="submitForm" :loading="isLoading"
          >Submit</el-button
        >
        <el-button :disabled="isLoading" @click="resetForm('form')"
          >Reset</el-button
        >
      </el-form-item>
    </el-form>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from "vue";
import type { FormInstance, FormRules } from "element-plus";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import { useI18n } from "vue-i18n";
import { ElNotification } from "element-plus";
import { PartyStatusTypes } from "@/core/types/PartyStatusTypes";
import CopyButton from "@/components/CopyButton.vue";
const { t } = useI18n();

const emits = defineEmits<{
  (event: "update:userDetails", userDetails: any): void;
}>();

const props = defineProps<{
  userDetails?: any;
  verificationDetails?: any;
}>();

const showSiteId = ref("");
const ruleFormRef = ref<FormInstance>();
const formData = ref<Record<string, any>>({});
const isLoading = ref(false);

const rules = reactive<FormRules>({
  email: [
    { required: true, message: "Please input email", trigger: "blur" },
    { type: "email", message: "Please input correct email", trigger: "blur" },
  ],
  firstName: [
    { required: true, message: "Please input first name", trigger: "blur" },
  ],
  lastName: [
    { required: true, message: "Please input last name", trigger: "blur" },
  ],
});

const accountDetails = ref({} as any);

const getUserInfos = async () => {
  try {
    isLoading.value = true;
    accountDetails.value = await UserService.getUserInfoByPartyId(
      partyId.value
    );
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    formData.value["referCode"] = accountDetails.value.referCode;
    isLoading.value = false;
  }
};
const submitForm = async () => {
  if (!ruleFormRef.value) return;
  await ruleFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        isLoading.value = true;
        const res = await TenantGlobalService.updateUserProfileInfo(
          partyId.value,
          formData.value
        );
        MsgPrompt.success("Update profile successfully");
        await resetForm(res);
        emits("update:userDetails", res);
      } catch (error) {
        MsgPrompt.error(error);
      } finally {
        isLoading.value = false;
      }
    } else {
      MsgPrompt.warning("Please fill in the required fields");
    }
  });
};

const updateStatus = async (newVal: any) => {
  try {
    isLoading.value = true;
    let form = {
      status: newVal,
    };
    await UserService.updateUserStatus(partyId.value, form);
    ElNotification({
      title: "Success",
      message: "Update status successfully",
      type: "success",
    });
  } catch (error) {
    ElNotification({
      title: "Error",
      message: "Update status failed",
      type: "error",
    });
  } finally {
    isLoading.value = false;
  }
};

const partyId = computed(
  () => props.verificationDetails?.partyId ?? props.userDetails?.partyId
);

const siteId = computed(() => props.verificationDetails?.siteId ?? 0);

const updateSiteID = async (_siteId: number) => {
  showSiteId.value = t("type.siteType." + _siteId);
};

const resetForm = async (_initialFormData?: any) => {
  if (!ruleFormRef.value) return;
  ruleFormRef.value.resetFields();
  var info = props.verificationDetails?.info ?? props.userDetails;
  if (props.verificationDetails) {
    info.phoneNumber = info.phone;
  }
  const initialForm = _initialFormData ?? info;
  formData.value = {
    ...initialForm,
    gender: parseInt(initialForm.gender),
  };
};

const options = Array.from({ length: 10000 }).map((_, idx) => ({
  value: `${idx + 1}`,
  label: `${idx + 1}`,
}));

onMounted(() => {
  resetForm(props.userDetails);

  showSiteId.value = t("type.siteType." + props.verificationDetails?.siteId);

  if (props.userDetails === undefined) {
    getUserInfos();
  } else {
    accountDetails.value = props.userDetails;
  }
});
</script>

<style scoped>
.socialMediaHead {
  width: 5px;
  height: 22px;
  margin-right: 10px;
  background-color: #ffd400;
}
</style>
