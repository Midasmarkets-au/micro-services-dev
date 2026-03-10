<template>
  <ModelForm
    :submit="submit"
    :discard="close"
    :title="$t('action.create')"
    elId="userCreate"
    :isLoading="isLoading"
    :submited="submited"
    :savingTitle="$t('action.saving')"
    ref="refUserCreateModel"
    :rules="rules"
    :formData="formData"
  >
    <div>
      <div class="fv-row mb-7">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.fullName") }}
        </label>
        <el-form-item prop="name">
          <el-input
            v-model="formData.name"
            name="user_name"
            type="text"
            :placeholder="$t('fields.fullName')"
          />
        </el-form-item>
      </div>

      <div class="fv-row mb-7">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("title.email") }}
        </label>
        <el-form-item prop="email">
          <el-input
            v-model="formData.email"
            name="user_email"
            type="text"
            placeholder="example@domain.com"
          />
        </el-form-item>
      </div>

      <div class="fv-row mb-7">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.language") }}
        </label>
        <el-form-item prop="lang">
          <el-input
            v-model="formData.lang"
            name="user_language"
            type="text"
            placeholder="en"
          />
        </el-form-item>
      </div>

      <div class="fv-row mb-7">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.timezone") }}
        </label>
        <el-form-item prop="timezone">
          <el-input
            v-model="formData.timezone"
            name="user_timezone"
            type="text"
            placeholder="Sydney/Australia"
          />
        </el-form-item>
      </div>
    </div>
  </ModelForm>
</template>

<script lang="ts" setup>
import { ref, inject } from "vue";
import ModelForm from "@/components/ModelForm.vue";
import ErrorMsg from "@/components/ErrorMsg";
import i18n from "@/core/plugins/i18n";
import { ElFormItem, ElInput } from "element-plus";
import { apiProviderKey } from "@/core/plugins/providerKeys";

const api = inject(apiProviderKey);

const refUserCreateModel = ref(null);
const isLoading = ref(true);
const submited = ref(true);

const emit = defineEmits(["created"]);

const formData = ref({
  name: "",
  email: "",
  lang: "",
  timezone: "",
});

const rules = ref({
  name: [
    {
      required: true,
      message: i18n.global.t("user.name_require"),
      trigger: "change",
    },
  ],
  email: [
    {
      required: true,
      message: i18n.global.t("user.email_require"),
      trigger: "change",
    },
  ],
  lang: [
    {
      required: true,
      message: i18n.global.t("user.lang_require"),
      trigger: "change",
    },
  ],
  timezone: [
    {
      required: true,
      message: i18n.global.t("user.tz_require"),
      trigger: "change",
    },
  ],
});

const submit = async () => {
  console.log(formData);
  isLoading.value = submited.value = true;

  await api["user.create"]({
    data: {
      name: formData.value.name,
      email: formData.value.email,
      lang: formData.value.lang,
      timezone: formData.value.timezone,
    },
  })
    .then(({ data }) => {
      isLoading.value = submited.value = false;
      emit("created", data.data);
      refUserCreateModel.value.hide();
    })
    .catch(({ response }) => {
      console.log(response);
      ErrorMsg.show(response);
      isLoading.value = submited.value = false;
    });
};

const show = () => {
  refUserCreateModel.value.show();
  isLoading.value = false;
  submited.value = false;
};

const close = () => {
  console.log("close");
};

const resetForm = () => {
  formData.value = {
    name: "",
    email: "",
    lang: "",
    timezone: "",
  };
  isLoading.value = false;
  submited.value = false;
};

defineExpose({
  resetForm,
  show,
});
</script>
