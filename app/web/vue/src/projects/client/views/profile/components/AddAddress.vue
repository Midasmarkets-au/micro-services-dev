<template>
  <el-dialog
    v-model="dialogRef"
    :title="title"
    align-center
    class="rounded-3 max-w-800px"
    :class="[isMobile ? 'w-100' : '']"
    :before-close="close"
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <div class="row">
        <el-form-item :label="t('fields.name')" prop="name" class="col-12">
          <el-input v-model="formData.name" />
        </el-form-item>
      </div>
      <div class="row">
        <el-form-item
          :label="t('fields.phone')"
          prop="phone"
          class="col-sm-6 col-12"
        >
          <el-input v-model="formData.phone">
            <template #prepend>
              <el-form-item class="regionCode" prop="ccc">
                <el-select v-model="formData.ccc" class="w-150px">
                  <el-option
                    v-for="item in regionCodes"
                    :key="item.code"
                    :label="item.name + ' ' + '+' + item.dialCode"
                    :value="item.dialCode"
                  ></el-option>
                </el-select>
              </el-form-item>
            </template>
          </el-input>
        </el-form-item>
        <el-form-item
          :label="t('fields.secondaryContact')"
          prop="content.socialMediaAccount"
          class="col-sm-6 col-12"
        >
          <el-input v-model="formData.content.socialMediaAccount">
            <template #prepend>
              <el-form-item class="regionCode" prop="content.socialMediaType">
                <el-select
                  v-model="formData.content.socialMediaType"
                  class="w-150px"
                >
                  <el-option
                    v-for="(item, index) in socialMediaTypes"
                    :key="index"
                    :label="t('fields.' + item.name)"
                    :value="item.name"
                  ></el-option>
                </el-select>
              </el-form-item>
            </template>
          </el-input>
        </el-form-item>
      </div>
      <div class="row">
        <el-form-item
          :label="t('fields.address')"
          prop="content.address"
          class="col-sm-6 col-12"
        >
          <el-input v-model="formData.content.address" />
        </el-form-item>
        <el-form-item
          :label="t('fields.city')"
          prop="content.city"
          class="col-sm-6 col-12"
        >
          <el-input v-model="formData.content.city" />
        </el-form-item>
      </div>
      <div class="row">
        <el-form-item
          :label="t('fields.state')"
          prop="content.state"
          class="col-sm-3 col-6"
        >
          <el-input v-model="formData.content.state" />
        </el-form-item>
        <el-form-item
          :label="t('fields.postalCode')"
          prop="content.postalCode"
          class="col-sm-3 col-6"
        >
          <el-input v-model="formData.content.postalCode" />
        </el-form-item>

        <el-form-item
          :label="t('fields.country')"
          prop="country"
          class="col-sm-6 col-12"
        >
          <el-select v-model="formData.country">
            <el-option
              v-for="item in regionCodes"
              :key="item.code"
              :label="item.name"
              :value="item.code"
            ></el-option>
          </el-select>
        </el-form-item>
      </div>
    </el-form>
    <template #footer>
      <div class="dialog-footer flex justify-end gap-2">
        <!-- <el-button
          class="btn btn-light btn-bordered btn-radius btn-lg d-flex"
          @click="dialogRef = false"
          >{{ $t("action.cancel") }}</el-button
        > 
        <el-button
          class="btn btn-primary btn-radius btn-lg d-flex"
          @click="submit(ruleFormRef)"
        >
          {{ $t("action.submit") }}
        </el-button>-->
        <button
          class="btn btn-light btn-bordered btn-radius btn-sm d-flex"
          @click="dialogRef = false"
        >
          {{ $t("action.cancel") }}
        </button>
        <button
          class="btn btn-primary btn-radius btn-sm d-flex"
          @click="submit(ruleFormRef)"
        >
          {{ $t("action.submit") }}
        </button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, reactive } from "vue";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import type { FormInstance, FormRules } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
import { getRegionCodes } from "@/core/data/phonesData";
import i18n from "@/core/plugins/i18n";
import { socialMediaTypes } from "@/core/types/socialMediaTypes";
const { t } = i18n.global;
const isLoading = ref(false);
const dialogRef = ref(false);
const ruleFormRef = ref<FormInstance>();
const isEdit = ref(false);
const title = ref(t("title.addAddress"));
const rules = reactive<FormRules>({
  name: [
    { required: true, message: t("error.NAME_IS_REQUIRED"), trigger: "blur" },
  ],
  phone: [
    {
      required: true,
      message: t("error.__PHONE_NUMBER_IS_REQUIRED__"),
      trigger: "blur",
    },
  ],
  country: [
    {
      required: true,
      message: t("error.__COUNTRY_IS_REQUIRED__"),
      trigger: "blur",
    },
  ],
  "content.address": [
    { required: true, message: t("error.addressIsRequired"), trigger: "blur" },
  ],
  "content.city": [
    {
      required: true,
      message: t("error.__CITY_IS_REQUIRED__"),
      trigger: "blur",
    },
  ],
  "content.postalCode": [
    {
      required: true,
      message: t("error.postalCodeisRequired"),
      trigger: "blur",
    },
  ],
  "content.state": [
    {
      required: true,
      message: t("error.stateOrProvinceRequired"),
      trigger: "blur",
    },
  ],
  "content.socialMediaAccount": [
    {
      required: true,
      message: t("tip.socialMediaError"),
      trigger: "blur",
    },
  ],
  "content.socialMediaType": [
    {
      required: true,
      message: t("tip.socialMediaError"),
      trigger: "blur",
    },
  ],
});
const regionCodes = ref(getRegionCodes());

const formData = ref<any>({
  name: "",
  ccc: regionCodes.value["au"].dialCode,
  phone: "",
  country: "",
  content: {
    address: "",
    city: "",
    postalCode: "",
    socialMediaType: socialMediaTypes[0].name,
  },
});
const emit = defineEmits<{
  (e: "submit"): void;
}>();

const submit = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;
  try {
    await formEl?.validate(async (valid, fields) => {
      if (valid) {
        if (isEdit.value) {
          await updateAddress();
        } else {
          await addAddress();
        }
        dialogRef.value = false;
        MsgPrompt.success(t("status.success"));
        emit("submit");
      }
    });
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const addAddress = async () => {
  formData.value.content = JSON.stringify(formData.value.content);
  await ClientGlobalService.addAddress(formData.value);
};

const updateAddress = async () => {
  formData.value.content = JSON.stringify(formData.value.content);
  await ClientGlobalService.updateAddress(
    formData.value.hashId,
    formData.value
  );
};

const show = (data?: any) => {
  if (data) {
    updateFormData(data);
    title.value = t("title.editAddress");
    isEdit.value = true;
  }
  dialogRef.value = true;
};

const close = () => {
  dialogRef.value = false;
  formData.value = {
    name: "",
    ccc: regionCodes.value["au"].dialCode,
    phone: "",
    country: "",
    content: {
      address: "",
      city: "",
      postalCode: "",
      socialMediaType: socialMediaTypes[0].name,
    },
  };
  title.value = t("title.addAddress");
  isEdit.value = false;
};

const updateFormData = (data: any) => {
  formData.value = data;
  formData.value.ccc = parseInt(data.ccc);
};

defineExpose({
  show,
});
</script>
