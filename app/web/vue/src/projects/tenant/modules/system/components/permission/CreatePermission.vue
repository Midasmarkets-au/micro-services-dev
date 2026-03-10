<template>
  <SimpleForm
    ref="modalRef"
    :title="type === 'create' ? 'Create Permission' : 'Edit Permission'"
    :is-loading="isSubmitting"
    :submit="submit"
    :before-close="hide"
  >
    <div class="px-5">
      <div>
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-position="top"
        >
          <el-form-item prop="key" label="Name">
            <el-input v-model="formData.key" clearable></el-input>
          </el-form-item>
          <el-form-item prop="auth" label="Auth">
            <el-switch v-model="formData.auth"></el-switch>
          </el-form-item>
          <el-form-item prop="category" label="Category">
            <el-select v-model="formData.category">
              <el-option
                v-for="item in PermissionCategory"
                :key="item"
                :label="item"
                :value="item"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item prop="method" label="Method">
            <el-select v-model="formData.method">
              <el-option
                v-for="item in ['GET', 'POST', 'PUT', 'DELETE']"
                :key="item"
                :label="item"
                :value="item"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item prop="action" label="Action">
            <el-input v-model="formData.action" clearable></el-input>
          </el-form-item>
        </el-form>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import type { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { PermissionCategory } from "@/core/types/PermissionCategory";
import SystemService from "../../services/SystemService";

const emits = defineEmits<{
  (e: "submit"): void;
}>();
const isSubmitting = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const type = ref("");

const hide = () => {
  formData.value = {
    itemId: 0,
    quantity: 1,
    points: 1,
  };
  modalRef.value?.hide();
};

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  key: [
    { required: true, message: "Please input permission key", trigger: "blur" },
  ],
  category: [
    { required: true, message: "Please input category", trigger: "blur" },
  ],
  method: [{ required: true, message: "Please input method", trigger: "blur" }],
  action: [{ required: true, message: "Please input action", trigger: "blur" }],
});

const formData = ref<any>({});

const show = async () => {
  modalRef.value?.show();
};

const submit = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;

  try {
    const res = await SystemService.createPermission(formData.value);
    MsgPrompt.success("Permission created successfully");
    emits("submit");
  } catch (error) {
    console.log(error);
  }
};

defineExpose({
  show,
  hide,
});
</script>

<style lang="scss">
.avatar-uploader .avatar {
  width: 178px;
  height: 178px;
  display: block;
}
.avatar-uploader .el-upload {
  border: 1px dashed var(--el-border-color);
  border-radius: 6px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: var(--el-transition-duration-fast);
}

.avatar-uploader .el-upload:hover {
  border-color: var(--el-color-primary);
}

.el-icon.avatar-uploader-icon {
  font-size: 28px;
  color: #8c939d;
  width: 178px;
  height: 178px;
  text-align: center;
}
</style>
