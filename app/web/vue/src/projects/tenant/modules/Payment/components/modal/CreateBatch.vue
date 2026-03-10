<template>
  <el-dialog
    v-model="modalRef"
    :title="t('action.createBatch')"
    width="800"
    align-center
    class="rounded"
  >
    <div>
      <el-form
        ref="ruleFormRef"
        :model="formData"
        :rules="rules"
        label-position="top"
      >
        <el-form-item :label="t('title.service')" prop="serviceId">
          <el-select
            v-model="formData.serviceId"
            clearable
            :placeholder="t('title.service')"
            class="w-150px"
          >
            <el-option
              v-for="item in tradeServices"
              :key="item.id"
              :label="item.description"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('fields.type')" prop="type">
          <el-select
            v-model="formData.type"
            clearable
            placeholder="Type"
            class="w-150px"
          >
            <el-option
              v-for="item in creditAdjustTypeOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('fields.note')" prop="note">
          <el-input
            v-model="formData.note"
            :autosize="{ minRows: 2, maxRows: 4 }"
            type="textarea"
          />
        </el-form-item>

        <el-form-item :label="t('action.batchUpload')" prop="file">
          <el-upload
            ref="uploadRef"
            class="upload-demo"
            :limit="1"
            :auto-upload="false"
            v-model:file-list="formData.file"
            :on-exceed="handleExceed"
          >
            <el-button type="primary">{{ $t("action.upload") }}</el-button>
          </el-upload>
        </el-form-item>
      </el-form>
    </div>

    <el-dialog
      v-model="submitSuccess"
      width="500"
      title="Bacth Detail"
      align-center
      append-to-body
      class="rounded"
    >
      <div>
        <p>
          {{ $t("fields.batchId") }}:
          <span>{{ successData.adjustBatchId }}</span>
        </p>
        <p>
          {{ $t("fields.totalAccountsUploaded") }}:
          <span>{{ successData.totalAccounts }}</span>
        </p>
        <p>
          {{ $t("fields.accountsInOurSystem") }}:
          <span>{{ successData.accountsInOurSystem }}</span>
        </p>
        <p>
          {{ $t("title.totalAmount") }}:
          <BalanceShow :balance="successData.totalAmount" />
        </p>
      </div>
      <div class="mt-5">
        <el-button type="danger" @click="close()">{{
          $t("action.close")
        }}</el-button>
        <el-button type="success" @click="process()" :disabled="isLoading">
          {{ $t("action.confirmToProcess") }}
        </el-button>
      </div>
    </el-dialog>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="modalRef = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit(ruleFormRef)">
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import type { FormInstance } from "element-plus";
import type { UploadInstance, UploadProps, UploadRawFile } from "element-plus";
import { genFileId } from "element-plus";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import { creditAdjustTypeOptions } from "@/core/types/CreditAdjustTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BalanceShow from "@/components/BalanceShow.vue";
import { useI18n } from "vue-i18n";

const t = useI18n().t;
const modalRef = ref(false);
const isLoading = ref(false);
const ruleFormRef = ref<FormInstance>();
const submitSuccess = ref(false);
const successData = ref<any>({});
const uploaded = ref(true);
const rules = reactive<any>({
  serviceId: [
    { required: true, message: "Please select service", trigger: "blur" },
  ],
  type: [{ required: true, message: "Please select type", trigger: "blur" }],
  note: [{ required: true, message: "Please input note", trigger: "blur" }],
  file: [{ required: true, message: "Please upload file", trigger: "blur" }],
});
const formData = ref<any>({});
const tradeServices = ref<any>([]);

const uploadRef = ref<UploadInstance>();

const handleExceed: UploadProps["onExceed"] = (files) => {
  if (uploadRef.value) {
    uploadRef.value.clearFiles();
  } else {
    console.error("uploadRef.value is null or undefined");
  }
  const file = files[0] as UploadRawFile;
  file.uid = genFileId();
  if (file && uploadRef.value) {
    uploadRef.value.handleStart(file);
  } else {
    console.error("uploadRef.value is null or undefined");
  }
};

const submit = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;

  try {
    await formEl?.validate(async (valid, fields) => {
      if (valid) {
        const form = new FormData();
        form.append("serviceId", formData.value.serviceId);
        form.append("type", formData.value.type);
        form.append("note", formData.value.note);
        if (formData.value.file) {
          const file = formData.value.file[0].raw;
          form.append("file", file);
        }
        const res = await PaymentService.uploadBatchFile(form);
        emit("fileUploaded");
        resetForm();
        successData.value = res;
        submitSuccess.value = true;
      }
    });
  } catch (e) {
    console.log(e);
    MsgPrompt.error("Submit failed");
    isLoading.value = false;
  }
  isLoading.value = false;
};

const resetForm = () => {
  ruleFormRef.value?.resetFields();
};

const emit = defineEmits<{
  (e: "fileUploaded"): void;
}>();
const show = () => {
  modalRef.value = true;
};
const process = async () => {
  isLoading.value = true;
  try {
    await PaymentService.confirmBatch(successData.value.adjustBatchId);
    emit("fileUploaded");
    MsgPrompt.success("Start process success");
  } catch (e) {
    MsgPrompt.error("Start process failed");
  }
  submitSuccess.value = false;
  modalRef.value = false;
  isLoading.value = false;
};
const close = () => {
  submitSuccess.value = false;
  modalRef.value = false;
};
const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await PaymentService.getTradeServices();
    tradeServices.value = res;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

onMounted(() => {
  fecthData();
});

defineExpose({
  show,
});
</script>
<style scoped>
:deep .el-upload-list__item-info {
  width: max-content;
}
.upload {
  color: #f56c6c;
  font-size: 12px;
}
</style>
