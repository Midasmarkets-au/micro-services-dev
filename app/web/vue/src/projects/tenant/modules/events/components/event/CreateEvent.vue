<template>
  <el-dialog
    v-model="modalRef"
    :title="$t('action.addEvent')"
    :before-close="hide"
    class="w-1000px"
    align-center
  >
    <div class="px-5" style="max-height: 900px; overflow-y: auto">
      <el-steps
        style="max-width: 800px"
        :active="step"
        finish-status="success"
        class="mb-5"
      >
        <el-step :title="$t('fields.basicInfo')" />
        <el-step :title="$t('fields.content')" />
        <el-step :title="$t('fields.pointsRules')" />
      </el-steps>
      <div>
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-position="top"
        >
          <div v-show="step == 0">
            <div class="row row-cols-2 g-5">
              <el-form-item :label="$t('fields.eventTitle')" prop="title">
                <el-input v-model="formData.title" :disabled="isSubmitting" />
              </el-form-item>
              <el-form-item
                :label="
                  $t('fields.eventKey') +
                  ' (Only English letter and number, no space)'
                "
                prop="key"
              >
                <el-input v-model="formData.key" :disabled="isSubmitting" />
                <el-tag type="error" class="mt-4" v-if="showKeyError">
                  Key already exists
                </el-tag>
              </el-form-item>
            </div>
            <div class="row row-cols-2 g-5">
              <el-form-item
                :label="$t('fields.accessRoles')"
                prop="accessRoles"
              >
                <el-checkbox-group
                  v-model="formData.accessRoles"
                  :disabled="isSubmitting"
                >
                  <el-checkbox
                    v-for="(role, index) in accessRoles"
                    :key="index"
                    :label="role.value"
                  >
                    {{ role.label }}
                  </el-checkbox>
                </el-checkbox-group>
              </el-form-item>

              <el-form-item :label="$t('fields.site')" prop="accessSites">
                <el-checkbox-group
                  v-model="formData.accessSites"
                  :disabled="isSubmitting"
                >
                  <el-checkbox
                    v-for="(role, index) in ConfigSiteTypesSelections"
                    :key="index"
                    :label="role.value"
                  >
                    {{ role.label }}
                  </el-checkbox>
                </el-checkbox-group>
              </el-form-item>
            </div>
            <div class="row row-cols-2 g-5">
              <el-form-item
                :label="$t('fields.applyStartDate')"
                props="applyStartOn"
              >
                <el-date-picker
                  v-model="formData.applyStartOn"
                  type="datetime"
                  placeholder="Apply Start Date"
                  :disabled="isSubmitting"
                />
              </el-form-item>
              <el-form-item
                :label="$t('fields.applyEndDate')"
                props="applyEndOn"
              >
                <el-date-picker
                  v-model="formData.applyEndOn"
                  type="datetime"
                  placeholder="Apply End Date"
                  :disabled="isSubmitting"
                />
              </el-form-item>
            </div>

            <div class="row row-cols-2 g-5">
              <el-form-item
                :label="$t('fields.eventStartDate')"
                props="startOn"
              >
                <el-date-picker
                  v-model="formData.startOn"
                  type="datetime"
                  placeholder="Event Start Date"
                  :disabled="isSubmitting"
                />
              </el-form-item>
              <el-form-item :label="$t('fields.eventEndDate')" props="endOn">
                <el-date-picker
                  v-model="formData.endOn"
                  type="datetime"
                  placeholder="Event End Date"
                  :disabled="isSubmitting"
                />
              </el-form-item>
            </div>
            <div class="row row-cols-2 g-5">
              <el-form-item
                :label="$t('action.uploadDesktopImage')"
                prop="image"
              >
                <UploadImage ref="uploadImageRef" />
              </el-form-item>
              <el-form-item
                :label="$t('action.uploadMobileImage')"
                prop="image"
              >
                <UploadImage ref="uploadMobileImageRef" />
              </el-form-item>
            </div>
          </div>
          <div v-show="step == 1">
            <div class="row row-cols-2 g-5">
              <el-form-item :label="$t('fields.language')" prop="language">
                <el-select v-model="formData.language" class="w-200px" disabled>
                  <el-option
                    v-for="item in langOptions"
                    :key="item.code"
                    :label="item.code + '   (' + item.name + ')'"
                    :value="item.code"
                  >
                    {{ item.code + "   (" + item.name + ")" }}
                  </el-option>
                </el-select>
              </el-form-item>
            </div>

            <el-form-item :label="$t('fields.content')" prop="description">
              <div>
                <quill-editor
                  theme="snow"
                  v-model:content="formData.description"
                  :modules="getModules(1)"
                  contentType="html"
                  toolbar="full"
                  :enable="!isSubmitting"
                />
              </div>
            </el-form-item>
            <el-form-item :label="$t('title.termsAndConditions')" prop="term">
              <div>
                <quill-editor
                  theme="snow"
                  v-model:content="formData.term"
                  :modules="getModules(2)"
                  contentType="html"
                  toolbar="full"
                  :enable="!isSubmitting"
                />
              </div>
            </el-form-item>
          </div>
          <div v-if="step == 2">
            <el-form-item :label="$t('fields.pointsRules')" prop="pointRules">
              <el-radio-group v-model="pointItem" :disabled="isSubmitting">
                <el-radio-button
                  v-for="(item, index) in PointRoles"
                  :key="index"
                  :label="item.value"
                  @click="switchPointsContent(item.value)"
                  ><div class="d-flex gap-1">
                    <div>{{ item.label }}</div>
                  </div>
                </el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item :label="$t('fields.content')" prop="description">
              <div>
                <quill-editor
                  theme="snow"
                  v-model:content="formData.instruction.pointsRule[pointItem]"
                  :modules="getModules(1)"
                  contentType="html"
                  toolbar="full"
                  :enable="!isSubmitting"
                />
              </div>
            </el-form-item>
          </div>
        </el-form>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <div v-if="step == 0">
          <el-button @click="modalRef = false" :disabled="isSubmitting">{{
            $t("action.cancel")
          }}</el-button>
          <el-button type="primary" @click="step++" :disabled="isSubmitting">
            {{ $t("action.next") }}
          </el-button>
        </div>
        <div v-else-if="step == 1">
          <el-button @click="step--" :disabled="isSubmitting">{{
            $t("action.back")
          }}</el-button>
          <el-button type="primary" @click="step++" :disabled="isSubmitting">
            {{ $t("action.next") }}
          </el-button>
        </div>
        <div v-else>
          <el-button @click="step--" :disabled="isSubmitting">{{
            $t("action.back")
          }}</el-button>
          <el-button type="primary" @click="submit" :loading="isSubmitting">
            {{ $t("action.confirm") }}
          </el-button>
        </div>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import type { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import EventServices from "../../services/EventsServices";
import { QuillEditor } from "@vueup/vue-quill";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import ImageUploader from "quill-image-uploader";
import BlotFormatter from "quill-blot-formatter/dist/BlotFormatter";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import { AccessRoles, PointRoles } from "@/core/types/shop/ShopEventRoles";
import UploadImage from "../shop/UploadImage.vue";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";

const emits = defineEmits<{
  (e: "submit"): void;
}>();
const isSubmitting = ref(false);
const showKeyError = ref(false);
const modalRef = ref(false);
const langOptions = ref<any>([]);
const contentList = ref({});
const uploadImageRef = ref<any>(null);
const uploadMobileImageRef = ref<any>(null);
const step = ref(0);
const pointItem = ref("all");
const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  name: [
    { required: true, message: "Please input item name", trigger: "blur" },
  ],
  title: [
    { required: true, message: "Please input item title", trigger: "blur" },
  ],
  key: [{ required: true, message: "Please input item key", trigger: "blur" }],
});

const accessRoles = AccessRoles;

const formData = ref<any>({
  name: "",
  accessRoles: ["Client", "Guest", "IB", "Sales"],
  accessSites: [],
  description: "",
  language: "en-us",
  instruction: {
    pointsRule: {
      all: "All",
      agent: "Agent",
      sales: "Sales",
      client: "Client",
    },
  },
  images: {},
});

const show = async () => {
  modalRef.value = true;
};

const hide = () => {
  modalRef.value = false;
  showKeyError.value = false;
  step.value = 0;
};

const successHide = () => {
  formData.value = {
    name: "",
    accessRoles: ["Client", "Guest", "IB", "Sales", "TenantAdmin"],
    accessSites: [],
    description: "",
    language: "en-us",
    instruction: {
      pointsRule: {
        all: "All",
        agent: "Agent",
        sales: "Sales",
        client: "Client",
      },
    },
    images: {},
  };
  hide();
};

const switchPointsContent = (lang: string) => {
  pointItem.value = lang;
};

const populateLangOptions = () => {
  const langKeys = Object.keys(contentList.value);
  langOptions.value = LanguageTypes.all.filter(
    (lang) => !langKeys.includes(lang.code)
  );
};

const processDefaultData = async () => {
  formData.value.name = formData.value.title;
  if (formData.value.key.includes(" ")) {
    formData.value.key = formData.value.key.replace(/\s/g, "-");
  }
  if (!formData.value.applyStartOn)
    formData.value.applyStartOn = new Date().toISOString();
  if (!formData.value.applyEndOn)
    formData.value.applyEndOn = new Date().toISOString();
  if (!formData.value.startOn)
    formData.value.startOn = new Date().toISOString();
  if (!formData.value.endOn) formData.value.endOn = new Date().toISOString();
};

const submit = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid) => {
    isValid = valid;
  });
  if (!isValid) return;

  isSubmitting.value = true;
  try {
    await processDefaultData();
    await uploadMobileImageRef.value?.uploadImage().then(() => {
      if (uploadMobileImageRef.value?.imageUrls.length > 0) {
        formData.value.images["mobile"] =
          uploadMobileImageRef.value?.imageUrls[0];
      }
    });

    await uploadImageRef.value
      ?.uploadImage()
      .then(() => {
        if (uploadImageRef.value?.imageUrls.length > 0) {
          formData.value.images["desktop"] = uploadImageRef.value?.imageUrls[0];
        }
      })
      .then(() => {
        //
      });
    await EventServices.createEvent(formData.value).then(() => {
      MsgPrompt.success("Event created successfully");
      emits("submit");
      successHide();
    });
  } catch (error) {
    console.log(error);
    if (error.response.data == "__KEY_ALREADY_EXISTS__") {
      step.value = 0;
      showKeyError.value = true;
    }
    MsgPrompt.error(error);
  }
  isSubmitting.value = false;
};

const uploadImg = async (file: any) => {
  const formData = new FormData();
  formData.append("file", file);
  const res = await EventServices.uploadImage(formData);
  return res["url"];
};

const getModules = (index: number) => {
  return [
    {
      name: "imageUploader" + index,
      module: ImageUploader,
      options: {
        upload: (file) => {
          return uploadImg(file);
        },
      },
    },
    {
      name: "blotFormatter" + index,
      module: BlotFormatter,
    },
  ];
};
onMounted(() => {
  populateLangOptions();
});
defineExpose({
  show,
  hide,
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

:deep .ql-container {
  min-height: 450px;
}
</style>
