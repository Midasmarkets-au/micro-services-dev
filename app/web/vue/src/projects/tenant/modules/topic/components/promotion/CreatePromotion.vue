<template>
  <el-drawer v-model="dialogRef" :title="$t('action.create')" size="80%">
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <el-collapse v-model="collapse">
        <el-collapse-item name="1">
          <template #title>
            <span class="fs-4">Promotion Time (China Time Zone)</span>
          </template>
          <div>
            <el-button @click="setMonth(1)">One Month</el-button>
            <el-button @click="setMonth(2)">Two Months</el-button>
          </div>
          <div class="row row-cols-2 g-5">
            <el-form-item label="from" prop="from">
              <el-date-picker
                v-model="formData.from"
                type="datetime"
                placeholder="Start Date"
                value-format="YYYY-MM-DD HH:mm:ss"
                format="YYYY-MM-DD HH:mm:ss"
                :disabled="isSubmitting"
              />
            </el-form-item>

            <el-form-item label="to" prop="to">
              <el-date-picker
                v-model="formData.to"
                type="datetime"
                placeholder="End Date"
                value-format="YYYY-MM-DD HH:mm:ss"
                format="YYYY-MM-DD HH:mm:ss"
                :disabled="isSubmitting"
              />
            </el-form-item>
          </div>
        </el-collapse-item>
      </el-collapse>
    </el-form>
    <p class="fs-4 mt-4">Language Detail</p>
    <div class="d-flex gap-4 mb-6">
      <div>
        <el-select v-model="selectedLanguage">
          <el-option
            v-for="item in LanguageTypes.all"
            :key="item.code"
            :label="item.code + '   (' + item.name + ')'"
            :value="item.code"
          />
        </el-select>
      </div>
      <el-button type="warning" @click="addLang" :disabled="canAddLang">
        {{ $t("action.addALanguage") }}</el-button
      >
    </div>
    <el-tabs type="card">
      <el-form ref="ruleFormRef" :model="formData" label-position="top">
        <el-tab-pane
          :label="index"
          v-for="(item, index) in data.contents"
          :key="index"
        >
          <el-form-item :label="$t('fields.title')" prop="title">
            <el-input v-model="item.title" :disabled="isSubmitting" />
          </el-form-item>
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('action.uploadImage')" prop="image">
              <UploadImage
                :ref="
                  (el) => {
                    if (el) uploadImageRefs[index] = el;
                  }
                "
                @image-uploaded="updateImage(index, $event)"
              />
            </el-form-item>
            <el-form-item label="PDF" prop="pdf">
              <UploadImage
                :ref="
                  (el) => {
                    if (el) uploadPdfRefs[index] = el;
                  }
                "
                accept="application/pdf"
                @image-uploaded="updatePdf(index, $event)"
              />
            </el-form-item>
          </div>
          <el-form-item :label="$t('fields.content')" prop="content">
            <div class="w-100">
              <jodit-editor v-model="item.content" :disabled="isSubmitting" />
            </div>
          </el-form-item>
        </el-tab-pane>
      </el-form>
    </el-tabs>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit">
          {{ $t("action.create") }}
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<script lang="ts" setup>
import { ref, reactive, computed } from "vue";
import type { FormInstance } from "element-plus";
import UploadImage from "./UploadImage.vue";
import "jodit/build/jodit.min.css";
import { JoditEditor } from "jodit-vue";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import { ElLoading } from "element-plus";
import SystemService from "../../../system/services/SystemService";
import { ElNotification } from "element-plus";
import notification from "@/core/plugins/notification";

const uploadImageRefs = ref<any>({});
const uploadPdfRefs = ref<any>({});
const dialogRef = ref(false);
const isSubmitting = ref(false);

const collapse = ref("1");
const promotionId = ref(0);

const emits = defineEmits<{
  (e: "submit"): void;
}>();

const formData = reactive({
  from: null,
  to: null,
});

const data = ref<any>({
  contents: {
    "en-us": {
      title: "",
      content: "",
      coverImage: "",
      pdfUrl: "",
    },
  },
});
const selectedLanguage = ref("en-us");
const canAddLang = computed(() => {
  return Object.keys(data.value.contents).includes(selectedLanguage.value);
});

const updateImage = (lang, url) => {
  data.value.contents[lang].coverImage = url;
};

const updatePdf = (lang, url) => {
  data.value.contents[lang].pdfUrl = url;
};

const submit = async () => {
  const validation = validateFormData();

  if (!validation.isValid) {
    // Display error messages
    validation.errors.forEach((error) => {
      ElNotification.error({
        title: "Validation Error",
        message: error,
        duration: 5000,
      });
    });
    return;
  }

  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    background: "rgba(0, 0, 0, 0.7)",
  });

  try {
    for (const lang in data.value.contents) {
      const langData = data.value.contents[lang];

      if (uploadImageRefs.value[lang]) {
        await uploadImageRefs.value[lang].uploadImage();
        if (uploadImageRefs.value[lang].returnUrl) {
          langData.coverImage = uploadImageRefs.value[lang].returnUrl;
        }
      }

      if (uploadPdfRefs.value[lang]) {
        await uploadPdfRefs.value[lang].uploadImage();
        if (uploadPdfRefs.value[lang].returnUrl) {
          langData.pdfUrl = uploadPdfRefs.value[lang].returnUrl;
        }
      }
    }

    const finalData = {
      ...formData,
      contents: data.value.contents,
    };
    console.log("finalData", finalData);

    await SystemService.createPromotionList(promotionId.value, finalData);
    emits("submit");
    notification.success();
    resetData();
    dialogRef.value = false;
  } catch (error) {
    console.error(error);
    notification.danger();
  } finally {
    loading.close();
  }
};

const validateFormData = () => {
  const errors = <any>[];

  // Validate dates
  if (!formData.from || !formData.to) {
    errors.push("Please set both from and to dates");
  }

  // Validate language content
  const emptyFields = <any>[];

  for (const lang in data.value.contents) {
    const langData = data.value.contents[lang];

    // Check if title is empty
    if (!langData.title?.trim()) {
      emptyFields.push(`Title for ${lang}`);
    }

    // Check if content is empty
    if (!langData.content?.trim()) {
      emptyFields.push(`Content for ${lang}`);
    }

    // Check if image is empty
    if (!langData.coverImage?.trim()) {
      emptyFields.push(`Cover image for ${lang}`);
    }

    // Check if PDF is empty
    if (!langData.pdfUrl?.trim()) {
      emptyFields.push(`PDF for ${lang}`);
    }
  }

  if (emptyFields.length > 0) {
    errors.push(
      `Please fill in all required fields: ${emptyFields.join(", ")}`
    );
  }

  return {
    isValid: errors.length === 0,
    errors,
  };
};

const addLang = () => {
  if (Object.keys(data.value.contents).length < LanguageTypes.all.length) {
    data.value.contents[selectedLanguage.value] = {
      title: "",
      content: "",
      coverImage: "",
      pdfUrl: "",
    };
  }
};

const show = (_id) => {
  dialogRef.value = true;
  promotionId.value = _id;
};

const resetData = () => {
  formData.from = null;
  formData.to = null;
  data.value.contents = {
    "en-us": {
      title: "",
      content: "",
      coverImage: "",
      pdfUrl: "",
    },
  };
  uploadImageRefs.value = {};
  uploadPdfRefs.value = {};
  selectedLanguage.value = "en-us";
};

const setMonth = (month: number) => {
  const currentDate = new Date();

  // Calculate the start month (current month + month parameter)
  const startMonth = currentDate.getMonth() + 1;
  const startYear = currentDate.getFullYear() + Math.floor(startMonth / 12);

  // Set from date to first day of the start month at 00:00:00 local time
  const fromDate = new Date(startYear, startMonth % 12, 1, 0, 0, 0);

  // Format as YYYY-MM-DD HH:mm:ss
  formData.from = formatDate(fromDate);

  // Calculate the end month (start month + 1)
  const endMonth = startMonth + month;
  const endYear = startYear + Math.floor(endMonth / 12);

  // Set to date to last day of the end month at 23:59:59 local time
  const toDate = new Date(endYear, endMonth % 12, 0, 23, 59, 59);

  // Format as YYYY-MM-DD HH:mm:ss
  formData.to = formatDate(toDate);
};

// Helper function to format date in YYYY-MM-DD HH:mm:ss format in local timezone
const formatDate = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
};

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  from: [
    { required: true, message: "Please select start date", trigger: "change" },
  ],
  to: [
    { required: true, message: "Please select end date", trigger: "change" },
  ],
});

defineExpose({
  show,
});
</script>
<style scoped>
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

:deep .jodit-workplace {
  min-height: 400px !important;
}
</style>
