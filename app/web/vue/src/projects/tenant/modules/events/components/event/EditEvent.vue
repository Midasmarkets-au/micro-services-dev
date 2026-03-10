<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.editEvent')"
    :is-loading="isSubmitting"
    :submit="submit"
    :before-close="hide"
    class="w-1000px"
  >
    <div class="px-5">
      <el-tabs
        v-model="tab"
        type="card"
        class="demo-tabs"
        @tab-click="changeTab"
      >
        <el-tab-pane
          v-for="(item, index) in tabList"
          :key="index"
          :label="item.label"
          :name="item.value"
          :disabled="isSubmitting"
        >
        </el-tab-pane>
      </el-tabs>
      <div>
        <el-form
          v-show="tab == 2"
          ref="detailRuleFormRef"
          :model="formData"
          :rules="detailRules"
          label-position="top"
          class="my-5"
        >
          <div class="row row-cols-2 g-5 mb-4">
            <el-form-item :label="$t('fields.eventKey')" prop="key">
              <el-input v-model="formData.key" :disabled="isSubmitting" />
            </el-form-item>
          </div>
          <div class="row row-cols-2 g-5 mb-4">
            <el-form-item :label="$t('fields.role')" prop="accessRoles">
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
          <div class="row row-cols-2 g-5 mb-4">
            <el-form-item
              :label="$t('fields.applyStartDate')"
              props="applyStartOn"
            >
              <el-date-picker
                v-model="formData.applyStartOn"
                type="datetime"
                :placeholder="$t('fields.applyStartDate')"
                :disabled="isSubmitting"
              />
            </el-form-item>
            <el-form-item :label="$t('fields.applyEndDate')" props="applyEndOn">
              <el-date-picker
                v-model="formData.applyEndOn"
                type="datetime"
                :placeholder="$t('fields.applyEndDate')"
                :disabled="isSubmitting"
              />
            </el-form-item>
          </div>

          <div class="row row-cols-2 g-5 mb-4">
            <el-form-item :label="$t('fields.eventStartDate')" props="startOn">
              <el-date-picker
                v-model="formData.startOn"
                type="datetime"
                :placeholder="$t('fields.eventStartDate')"
                :disabled="isSubmitting"
              />
            </el-form-item>
            <el-form-item :label="$t('fields.eventEndDate')" props="endOn">
              <el-date-picker
                v-model="formData.endOn"
                type="datetime"
                :placeholder="$t('fields.eventEndDate')"
                :disabled="isSubmitting"
              />
            </el-form-item>
          </div>
        </el-form>
        <el-form
          v-show="tab == 1"
          ref="ruleFormRef"
          :model="languageFormData"
          :rules="rules"
          label-position="top"
        >
          <el-form-item :label="$t('fields.language')" prop="language">
            <el-radio-group v-model="langItem" :disabled="isSubmitting">
              <el-radio-button
                v-for="item in langOptions"
                :key="item.code"
                :label="item.code"
                @click="switchLang(item.code)"
              >
                <div class="d-flex gap-1">
                  <div>{{ item.code }}</div>
                  <div v-if="item.isAdded">
                    <el-icon><Check /></el-icon>
                  </div>
                </div>
              </el-radio-button>
            </el-radio-group>
          </el-form-item>
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('fields.eventTitle')" prop="title">
              <el-input
                v-model="languageFormData.title"
                :disabled="isSubmitting"
              />
            </el-form-item>
          </div>
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('action.uploadDesktopImage')">
              <UploadImage ref="uploadImageRef" />
            </el-form-item>
            <el-form-item :label="$t('action.uploadMobileImage')">
              <UploadImage ref="uploadMobileImageRef" />
            </el-form-item>
          </div>
          <el-radio-group v-model="contentPointRuleSwitch" class="mb-4">
            <el-radio-button :label="true">{{
              $t("fields.content")
            }}</el-radio-button>
            <el-radio-button :label="false">{{
              $t("fields.pointsRules")
            }}</el-radio-button>
          </el-radio-group>
          <div v-if="contentPointRuleSwitch">
            <el-form-item :label="$t('fields.content')" prop="description">
              <div>
                <quill-editor
                  theme="snow"
                  v-model:content="languageFormData.description"
                  :modules="getModules(2)"
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
                  v-model:content="languageFormData.term"
                  :modules="getModules(1)"
                  contentType="html"
                  toolbar="full"
                  :enable="!isSubmitting"
                />
              </div>
            </el-form-item>
          </div>
          <div v-else>
            <el-radio-group
              v-model="pointItem"
              :disabled="isSubmitting"
              class="mb-2"
            >
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
            <el-form-item prop="description">
              <div>
                <quill-editor
                  theme="snow"
                  v-model:content="
                    languageFormData.instruction.pointsRule[pointItem]
                  "
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
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import type { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import EventServices from "../../services/EventsServices";
import { QuillEditor } from "@vueup/vue-quill";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import ImageUploader from "quill-image-uploader";
import BlotFormatter from "quill-blot-formatter/dist/BlotFormatter";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import UploadImage from "../shop/UploadImage.vue";
import type { TabsPaneContext } from "element-plus";
import { AccessRoles, PointRoles } from "@/core/types/shop/ShopEventRoles";
import { Check } from "@element-plus/icons-vue";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";
const emits = defineEmits<{
  (e: "submit"): void;
}>();
const isSubmitting = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const langOptions = ref(LanguageTypes.all);
const langItem = ref<any>("");
const contentList = ref({});
const uploadImageRef = ref<any>(null);
const uploadMobileImageRef = ref<any>(null);
const ruleFormRef = ref<FormInstance>();
const detailRuleFormRef = ref<FormInstance>();
const contentPointRuleSwitch = ref(true);
const imageList = ref<any>([]);
const pointItem = ref("all");
const rules = reactive<any>({
  name: [
    { required: true, message: "Please input item name", trigger: "blur" },
  ],
  title: [
    { required: true, message: "Please input item title", trigger: "blur" },
  ],
  description: [
    {
      required: true,
      message: "Please input item description",
      trigger: "blur",
    },
  ],
});

const detailRules = reactive<any>({
  key: [{ required: true, message: "Please input key", trigger: "blur" }],
  accessRoles: [
    { required: true, message: "Please select access roles", trigger: "blur" },
  ],
  applyStartOn: [
    {
      required: true,
      message: "Please select apply start date",
      trigger: "blur",
    },
  ],
  applyEndOn: [
    {
      required: true,
      message: "Please select apply end date",
      trigger: "blur",
    },
  ],
  startOn: [
    {
      required: true,
      message: "Please select event start date",
      trigger: "blur",
    },
  ],
  endOn: [
    {
      required: true,
      message: "Please select event end date",
      trigger: "blur",
    },
  ],
});

const tab = ref(1);

const tabList = ref<any>([
  { label: "Content", value: 1 },
  { label: "Detail", value: 2 },
]);

const changeTab = (tab: TabsPaneContext, event: Event) => {
  tab.value = tab.props.name;
};

const accessRoles = AccessRoles;

const formData = ref<any>({});
const languageFormData = ref<any>({});

const show = async (_item: any) => {
  fetchData(_item.id);
  modalRef.value?.show();
};

const switchPointsContent = (lang: string) => {
  pointItem.value = lang;
};

const fetchData = async (id: number) => {
  isSubmitting.value = true;
  try {
    const res = await EventServices.queryEventById(id);
    formData.value = res;
    await fetchContent();
  } catch (error) {
    console.log(error);
  } finally {
    isSubmitting.value = false;
  }
};

const fetchContent = async () => {
  langItem.value = formData.value.languages[0].language;
  languageFormData.value = formData.value.languages[0];

  await fetchImage(languageFormData.value);
  const contents = formData.value.languages;
  for (let i = 0; i < contents.length; i++) {
    contentList.value[contents[i].language] = contents[i];
    languageExists(contents[i].language);
  }
};

const languageExists = (lang: string) => {
  let item = langOptions.value.find((item) => item.code == lang);
  if (item) {
    item.isAdded = true;
  }
};

const fetchImage = async (data: any) => {
  if (Object.keys(data.images).length > 0) {
    if (imageList.value[langItem.value]) {
      uploadImageRef.value.imageUrl =
        imageList.value[langItem.value]["desktop"];
      uploadMobileImageRef.value.imageUrl =
        imageList.value[langItem.value]["mobile"];
      return;
    }
    isSubmitting.value = true;

    if (data.images["desktop"]) {
      uploadImageRef.value.uploading = true;
      const imageUrl = await EventServices.getImagesWithGuid(
        data.images["desktop"]
      );
      addImage(langItem.value, "desktop", imageUrl);
      uploadImageRef.value.imageUrl = imageUrl;
      uploadImageRef.value.uploading = false;
    }

    if (data.images["mobile"]) {
      uploadMobileImageRef.value.uploading = true;
      const mobileImageUrl = await EventServices.getImagesWithGuid(
        data.images["mobile"]
      );
      addImage(langItem.value, "mobile", mobileImageUrl);
      uploadMobileImageRef.value.imageUrl = mobileImageUrl;
      uploadMobileImageRef.value.uploading = false;
    }
    isSubmitting.value = false;
  } else {
    uploadImageRef.value.imageUrl = "";
    uploadMobileImageRef.value.imageUrl = "";
  }
};

const addImage = (lang: string, deviceType: string, imageUrl: string) => {
  if (!imageList.value[lang]) {
    imageList.value[lang] = {};
  }
  imageList.value[lang][deviceType] = imageUrl;
};

const switchLang = async (lang: string) => {
  if (langItem.value == lang) return;
  langItem.value = lang;
  const item = contentList.value[lang];
  if (item == null || item == undefined) {
    contentList.value[lang] = {
      name: "",
      title: "",
      description: "<p>description</p>",
      term: "<p>term</p>",
      images: {},
      instruction: {
        pointsRule: {
          all: "<p>all</p>",
          agent: "<p>agent</p>",
          sales: "<p>sales</p>",
          client: "<p>client</p>",
        },
      },
    };
    languageFormData.value = contentList.value[lang];
  } else {
    languageFormData.value = item;
  }
  await fetchImage(languageFormData.value);
};

const submit = async () => {
  isSubmitting.value = true;
  if (tab.value === 1) {
    await submitLanguage();
  } else {
    await submitDetail();
  }
  isSubmitting.value = false;
};

const submitLanguage = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;

  try {
    languageFormData.value.eventId = formData.value.eventId;
    languageFormData.value.name = languageFormData.value.title;

    await uploadMobileImageRef.value?.uploadImage().then(() => {
      if (uploadMobileImageRef.value?.imageUrls.length > 0) {
        languageFormData.value.images["mobile"] =
          uploadMobileImageRef.value?.imageUrls[0];
      }
    });

    await uploadImageRef.value
      ?.uploadImage()
      .then(() => {
        if (uploadImageRef.value?.imageUrls.length > 0) {
          languageFormData.value.images["desktop"] =
            uploadImageRef.value?.imageUrls[0];
        }
      })
      .then(() => {
        EventServices.updateEventLanguage(
          formData.value.id,
          langItem.value,
          languageFormData.value
        );
        MsgPrompt.success(langItem.value + " updated successfully");
        languageExists(langItem.value);
        emits("submit");
      });
  } catch (error) {
    console.log(error);
    MsgPrompt.error("Update failed");
    hide();
  }
};

const submitDetail = async () => {
  if (!detailRuleFormRef.value) return;

  let isValid = false;
  await detailRuleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;

  try {
    await EventServices.updateEvent(formData.value.id, formData.value);
    MsgPrompt.success("Event updated successfully");
  } catch (error) {
    console.log(error);
    MsgPrompt.error("Update failed");
  }
  emits("submit");
};

const hide = () => {
  modalRef.value?.hide();
  contentPointRuleSwitch.value = true;
  tab.value = 1;
  formData.value = {};
  languageFormData.value = {};
  contentList.value = {};
  imageList.value = {};
  uploadImageRef.value.imageUrl = "";
  uploadMobileImageRef.value.imageUrl = "";
  langItem.value = "";
  langOptions.value.forEach((item) => {
    item.isAdded = false;
  });
};

const uploadImg = async (file: any) => {
  const formData = new FormData();
  formData.append("file", file);
  const res = await EventServices.uploadImage(formData);
  return res["url"];
};

const modules = [
  {
    name: "imageUploader",
    module: ImageUploader,
    options: {
      upload: (file) => {
        return uploadImg(file);
      },
    },
  },
  {
    name: "blotFormatter",
    module: BlotFormatter,
  },
];

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
