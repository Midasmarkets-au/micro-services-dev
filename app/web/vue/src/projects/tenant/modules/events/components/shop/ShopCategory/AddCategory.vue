<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.addCategory')"
    width="800"
    align-center
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <el-form-item :label="$t('fields.key')" prop="key" class="fw-bold">
        <el-input
          v-model="formData.key"
          placeholder="Enter key"
          :disabled="isLoading"
          @keypress="handleKeypress"
        ></el-input>
      </el-form-item>
      <hr />

      <h4>{{ $t("title.translation") }}</h4>

      <div
        v-for="(item, index) in LanguageTypes.all"
        :key="index"
        class="row row-cols-2"
      >
        <el-form-item
          :label="item.name + ' (' + item.code + ')'"
          prop="languages"
        >
          <el-input
            v-model="languages[item.code]"
            placeholder="Enter translation"
            :disabled="isLoading"
          />
        </el-form-item>
      </div>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading" plain>{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="success" @click="submit" :loading="isLoading" plain>
          {{ $t("action.confirm") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import type { FormInstance } from "element-plus";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import notification from "@/core/plugins/notification";
import EventsServices from "../../../services/EventsServices";

const emits = defineEmits<{
  (e: "submit"): void;
}>();

const formData = ref<any>({
  key: "",
});
const keys = ref<any>([]);
const languages = ref<any>({});
const dialogRef = ref(false);
const isLoading = ref(false);
const ruleFormRef = ref<FormInstance>();
const show = (_keys: any) => {
  dialogRef.value = true;
  keys.value = _keys;
};

const handleKeypress = (event) => {
  const char = String.fromCharCode(event.keyCode);
  if (!/[a-zA-Z0-9]/.test(char)) {
    event.preventDefault();
  }
};

const submit = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  isLoading.value = true;
  try {
    formData.value.languages = languages.value;
    formData.value.key = formData.value.key.trim();
    if (
      formData.value.languages &&
      !Object.keys(formData.value.languages).includes("en-us")
    ) {
      throw new Error("English translation is required");
    }
    await EventsServices.updateShopCategory(formData.value);
    reset();
    dialogRef.value = false;
    emits("submit");
    notification.success();
  } catch (error) {
    console.error("Validation failed:", error);
    notification.danger(error);
  }
  isLoading.value = false;
};

const reset = () => {
  formData.value.key = "";
  languages.value = {};
  ruleFormRef.value?.resetFields();
};

const rules = {
  key: [
    { required: true, message: "Please input the key", trigger: "blur" },
    { min: 3, max: 20, message: "Length should be 3 to 20", trigger: "blur" },
    {
      validator: (rule: any, value: string, callback: any) => {
        if (keys.value.includes(value)) {
          callback(new Error("Key must be unique"));
        } else {
          callback();
        }
      },
      trigger: "blur",
    },
  ],
};

defineExpose({
  show,
});
</script>
