<template>
  <el-dialog
    :title="edit == false ? 'Add Blocked User' : 'Edit Blocked User'"
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
        <el-form-item label="Name" prop="name">
          <el-input
            v-model="formData.name"
            placeholder="Name"
            clearable
            :disable="isLoading"
          ></el-input>
        </el-form-item>
        <el-form-item label="Phone" prop="phone" class="mt-5">
          <el-input
            v-model="formData.phone"
            placeholder="Phone"
            clearable
            :disable="isLoading"
          ></el-input>
        </el-form-item>
        <el-form-item label="Email" prop="email" class="mt-5">
          <el-input
            v-model="formData.email"
            placeholder="Email"
            clearable
            :disable="isLoading"
          ></el-input>
        </el-form-item>
        <el-form-item label="ID Number" prop="idNumber" class="mt-5">
          <el-input
            v-model="formData.idNumber"
            placeholder="ID Number"
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
import ToolServices from "../services/ToolServices";

const isLoading = ref(false);
const createModalRef = ref(false);
const edit = ref(false);
const formRef = ref<FormInstance>();
const formData = ref<any>({
  name: "",
  phone: "",
  email: "",
  idNumber: "",
});
const formRule = reactive<any>({
  name: [{ required: true, message: "Please input name", trigger: "blur" }],
  phone: [{ required: true, message: "Please input phone", trigger: "blur" }],
  email: [{ required: true, message: "Please input email", trigger: "blur" }],
  idNumber: [
    { required: true, message: "Please input idNumber", trigger: "blur" },
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
      await ToolServices.addBlockedUser(formData.value);
    } else {
      await ToolServices.editBlockedUser(formData.value.id, formData.value);
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
    formData.value.name = item.name;
    formData.value.phone = item.phone;
    formData.value.email = item.email;
    formData.value.idNumber = item.idNumber;
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
