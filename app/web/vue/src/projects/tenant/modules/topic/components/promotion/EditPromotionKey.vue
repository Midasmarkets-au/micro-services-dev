<template>
  <el-dialog
    v-model="dialogRef"
    title="Edit Promotion Key"
    width="500"
    align-center
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <el-form-item :label="$t('fields.key')" prop="title">
        <el-input v-model="formData.key" :disabled="isSubmitting" />
      </el-form-item>

      <el-form-item :label="$t('fields.sites')" prop="access_sites">
        <el-checkbox-group
          v-model="formData.access_sites"
          :disabled="isSubmitting"
        >
          <el-checkbox
            v-for="(tenant, index) in websiteTenant"
            :key="index"
            :label="tenant.value"
          >
            {{ tenant.label }}
          </el-checkbox>
        </el-checkbox-group>
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isSubmitting">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit" :loading="isSubmitting">
          {{ $t("action.update") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref, reactive } from "vue";
import type { FormInstance } from "element-plus";
import { websiteTenant } from "@/core/types/TenantTypes";
import SystemService from "../../../system/services/SystemService";
const dialogRef = ref(false);
const isSubmitting = ref(false);
const promotionId = ref(0);
const ruleFormRef = ref<FormInstance>();
const formData = reactive({
  key: "",
  access_sites: ["bvi"],
});

const rules = reactive<any>({
  key: [{ required: true, message: "Please input item key", trigger: "blur" }],
  access_sites: [
    {
      required: true,
      message: "Please select access sites",
      trigger: "change",
    },
  ],
});

const emits = defineEmits<{
  (e: "submit"): void;
}>();

const submit = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;

  isSubmitting.value = true;
  try {
    const res = await SystemService.editPromotionKey(
      promotionId.value,
      formData
    );
    if (res) {
      dialogRef.value = false;
      formData.key = "";
      formData.access_sites = ["bvi"];
      emits("submit");
    }
  } catch (error) {
    console.error(error);
  } finally {
    isSubmitting.value = false;
  }
};

const show = (item) => {
  dialogRef.value = true;
  promotionId.value = item.id;
  formData.key = item.key;
  formData.access_sites = item.access_sites;
};

defineExpose({
  show,
});
</script>
