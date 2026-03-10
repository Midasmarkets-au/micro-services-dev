<template>
  <el-dialog
    :title="edit == false ? $t('action.addCheaterIp') : 'Edit Cheater IP'"
    width="500"
    align-center
    v-model="createModalRef"
    :before-close="close"
  >
    <div class="pt-5">
      <el-form
        :model="formData"
        :rules="formRule"
        ref="formRef"
        label-width="120px"
      >
        <el-form-item label="IP" prop="ip">
          <el-input
            v-model="formData.ip"
            placeholder="IP"
            clearable
            :disable="isLoading"
          ></el-input>
        </el-form-item>

        <el-form-item label="Note" prop="note" class="mt-5">
          <el-input
            v-model="formData.note"
            placeholder="Note"
            clearable
            :disable="isLoading"
          ></el-input>
        </el-form-item>

        <div class="text-end mt-15">
          <el-button
            type="danger"
            @click.prevent="createModalRef = false"
            :disable="isLoading"
          >
            {{ $t("action.close") }}
          </el-button>
          <el-button
            type="success"
            :disable="isLoading"
            @click="addCheaterIp(formRef)"
          >
            {{ $t("action.submit") }}
          </el-button>
        </div>
      </el-form>
    </div>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import ToolServices from "../services/ToolServices";

const isLoading = ref(false);
const createModalRef = ref(false);
const edit = ref(false);
const formRef = ref<FormInstance>();
const formData = ref<any>({
  ip: "",
  note: "",
});
const formRule = reactive<any>({
  ip: [{ required: true, message: "Please input IP", trigger: "blur" }],
  note: [
    { required: true, message: "Please input description", trigger: "blur" },
  ],
});

const addCheaterIp = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;
  if (!formEl) return;
  await formEl?.validate((valid) => {
    if (valid) {
      submit();
    } else {
      return false;
    }
  });
  isLoading.value = false;
};

const submit = async () => {
  isLoading.value = true;
  try {
    if (edit.value == false) {
      await ToolServices.addCheaterIp({
        ip: formData.value.ip,
        note: formData.value.note,
      });
    } else {
      await ToolServices.updateCheaterIp(formData.value.id, {
        ip: formData.value.ip,
        note: formData.value.note,
        enable: formData.value.enable,
      });
    }
    MsgPrompt.success("Success");
    emits("eventSubmit");
    close();
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

const show = (item?: any) => {
  createModalRef.value = true;
  if (item) {
    formData.value.id = item.id;
    formData.value.enable = item.enable;
    formData.value.ip = item.ip;
    formData.value.note = item.note;
    edit.value = true;
  }
};

const clear = (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  formEl.resetFields();
};

const close = () => {
  edit.value = false;
  clear(formRef.value);
  createModalRef.value = false;
};

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

defineExpose({ show });
</script>
