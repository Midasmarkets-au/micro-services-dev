<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.edit')"
    :is-loading="isSubmitting"
    :submit="submit"
    :before-close="hide"
    :save-title="
      tab == 1
        ? `${$t('action.submit')} ${langItem}`
        : $t('action.submitDetail')
    "
    :submit-color="tab == 1 ? 'primary' : 'success'"
    class="w-1000px"
  >
    <div class="px-5">
      <div>
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
            :disabled="isSubmitting || uploadImageRef.uploading"
          >
          </el-tab-pane>
        </el-tabs>
        <el-form
          :model="formData"
          label-position="top"
          v-show="tab == 2"
          class="mb-10"
        >
          <div class="row row-cols-2 g-5 my-4">
            <el-form-item :label="$t('fields.points')" prop="point">
              <el-input-number
                v-model="pointsInDollar"
                :min="1"
                :disabled="isSubmitting"
              />
            </el-form-item>
            <el-form-item :label="$t('fields.role')" prop="accessRoles">
              <el-checkbox-group
                v-model="formData.accessRoles"
                :min="1"
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
          </div>
          <div
            class="row row-cols-2 g-5 my-4"
            v-if="formData.type == EventShopItemTypes.Product"
          >
            <el-form-item :label="$t('fields.category')" prop="category">
              <el-select
                v-model="formData.category"
                class="w-200px"
                :disabled="isSubmitting"
              >
                <el-option
                  v-for="item in props.categoryData"
                  :key="item.value"
                  :label="props.getCategoryName(item.value)"
                  :value="item.value"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('fields.sites')" prop="accessSites">
              <el-checkbox-group
                v-model="formData.accessSites"
                :disabled="isSubmitting"
              >
                <el-checkbox
                  v-for="(site, index) in ConfigSiteTypesSelections"
                  :key="index"
                  :label="site.value"
                >
                  {{ site.label }}
                </el-checkbox>
              </el-checkbox-group>
            </el-form-item>
          </div>
          <div v-else class="row row-cols-2 g-5">
            <div>
              <el-form-item
                :label="$t('fields.cardPointsLevel')"
                prop="rewardTypes"
              >
                <el-select
                  v-model="formData.configuration.rewardType"
                  class="w-200px"
                  :disabled="isSubmitting"
                >
                  <el-option
                    v-for="item in RewardTypesOptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  />
                </el-select>
              </el-form-item>
              <el-form-item :label="$t('fields.rewardRoles')" prop="type">
                <el-select
                  v-model="formData.type"
                  class="w-200px"
                  :disabled="isSubmitting"
                >
                  <el-option
                    v-for="item in RewardRolesOptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  />
                </el-select>
              </el-form-item>
              <el-form-item :label="$t('fields.validDays')">
                <el-input-number
                  v-model="formData.configuration.validPeriodInDays"
                  :disabled="isSubmitting"
                />
              </el-form-item>
            </div>
            <div>
              <el-form-item :label="$t('fields.sites')" prop="accessSites">
                <el-checkbox-group
                  v-model="formData.accessSites"
                  :disabled="isSubmitting"
                >
                  <el-checkbox
                    v-for="(site, index) in ConfigSiteTypesSelections"
                    :key="index"
                    :label="site.value"
                    :value="site.value"
                  >
                    {{ site.label }}
                  </el-checkbox>
                </el-checkbox-group>
              </el-form-item>
              <el-form-item
                :label="$t('action.uploadBannerImage')"
                prop="image"
              >
                <UploadImage ref="uploadBannerRef" />
              </el-form-item>
            </div>
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
                ><div class="d-flex gap-1">
                  <div>{{ item.code }}</div>
                  <div v-if="item.isAdded">
                    <el-icon><Check /></el-icon>
                  </div>
                </div>
              </el-radio-button>
            </el-radio-group>
          </el-form-item>
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('fields.itemTitle')" prop="title">
              <el-input
                v-model="languageFormData.title"
                :disabled="isSubmitting"
              />
            </el-form-item>
          </div>
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('action.uploadCoverImage')" prop="image">
              <div>
                <UploadImage ref="uploadImageRef" />
                <div v-if="showImageError == true">
                  <p class="text-danger">{{ $t("error.fieldIsRequired") }}</p>
                </div>
              </div>
            </el-form-item>
            <div v-if="formData.type != EventShopItemTypes.Product"></div>
          </div>
          <el-form-item :label="$t('fields.content')" prop="description">
            <div v-if="isContentEmpty">
              <p class="text-danger">{{ $t("tip.contentIsRequired") }}</p>
            </div>
            <div>
              <quill-editor
                theme="snow"
                v-model:content="languageFormData.description"
                :modules="modules"
                contentType="html"
                toolbar="full"
                :enable="!isSubmitting"
              />
            </div>
          </el-form-item>
        </el-form>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive, nextTick } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import type { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import EventServices from "../../services/EventsServices";
import { QuillEditor } from "@vueup/vue-quill";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import ImageUploader from "quill-image-uploader";
import BlotFormatter from "quill-blot-formatter/dist/BlotFormatter";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import {
  EventShopItemCategoryOptions,
  EventShopItemCategory,
  EventShopItemTypes,
  RewardTypesOptions,
  EventShopRewardTypes,
  RewardRolesOptions,
} from "@/core/types/shop/ShopItemTypes";
import UploadImage from "./UploadImage.vue";
import type { TabsPaneContext } from "element-plus";
import { AccessRoles } from "@/core/types/shop/ShopEventRoles";
import Decimal from "decimal.js";
import { Check } from "@element-plus/icons-vue";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";

const emits = defineEmits<{
  (e: "submit"): void;
}>();

const props = defineProps<{
  categoryData?: any;
  getCategoryName?: any;
}>();

const isSubmitting = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const langOptions = ref(LanguageTypes.all);
const langItem = ref<any>("");
const contentList = ref({});
const uploadImageRef = ref<any>(null);
const uploadBannerRef = ref<any>(null);
const ruleFormRef = ref<FormInstance>();
const showImageError = ref(false);
const isContentEmpty = ref(false);
const imageList = ref<any>([]);
const pointsInDollar = ref(0);
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

const tab = ref(1);

const tabList = ref<any>([
  { label: "Content", value: 1 },
  { label: "Detail", value: 2 },
]);

const changeTab = (tab: TabsPaneContext, event: Event) => {
  tab.value = tab.props.name;
};

const accessRoles = AccessRoles;

const formData = ref<any>({
  point: 1,
  name: "",
  type: EventShopItemTypes.Product,
  category: EventShopItemCategory.bcrMerch,
  accessRoles: ["Client", "IB", "Sales"],
  accessSites: [],
  description: "",
  language: "en-us",
  configuration: {
    validPeriodInDays: 0,
    rewardType: EventShopRewardTypes.Point1000,
  },
});
const languageFormData = ref<any>({});

const show = async (_item: any) => {
  fetchData(_item.id);
  modalRef.value?.show();
};

const fetchData = async (id: number) => {
  isSubmitting.value = true;
  try {
    await EventServices.queryShopItemById(id).then((res) => {
      formData.value = res;
      console.log("formData", formData.value);
      pointsInDollar.value = new Decimal(formData.value.point)
        .div(10000)
        .toNumber();
    });
    await fetchContent();
  } catch (error) {
    console.log(error);
  } finally {
    isSubmitting.value = false;
  }
};

const fetchContent = async () => {
  langItem.value = formData.value.languageList[0].language;
  languageFormData.value = formData.value.languageList[0];

  await fetchImage(languageFormData.value);
  await nextTick();
  fetchBannerImage();
  const contents = formData.value.languageList;
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

const fetchBannerImage = async () => {
  if (formData.value.configuration.bannerImages) {
    const bannerImageGuid = formData.value.configuration.bannerImages[0];
    uploadBannerRef.value.uploading = true;
    const bannerImageUrl = await EventServices.getImagesWithGuid(
      bannerImageGuid
    );
    uploadBannerRef.value.imageUrl = bannerImageUrl;
    uploadBannerRef.value.uploading = false;
  }
};
const fetchImage = async (data: any) => {
  if (data.images.length > 0) {
    if (imageList.value[langItem.value]) {
      await uploadImageRef.value.handleRemove();
      uploadImageRef.value.imageUrl = imageList.value[langItem.value];
      return;
    }
    uploadImageRef.value.uploading = true;
    const imageUrl = await EventServices.getImagesWithGuid(data.images[0]);
    imageList.value[langItem.value] = imageUrl;
    uploadImageRef.value.imageUrl = imageUrl;
    uploadImageRef.value.uploading = false;
  } else {
    uploadImageRef.value.imageUrl = "";
    uploadImageRef.value.handleRemove();
  }
};
const switchLang = (lang: string) => {
  if (langItem.value == lang) return;
  langItem.value = lang;
  const item = contentList.value[lang];
  if (item == null || item == undefined) {
    contentList.value[lang] = {
      name: "",
      title: "",
      description: "<p>content</p>",
      images: [],
    };
    languageFormData.value = contentList.value[lang];
  } else {
    languageFormData.value = item;
  }
  fetchImage(languageFormData.value);
};

const checkContent = () => {
  if (languageFormData.value.description == "<p>content</p>") {
    isContentEmpty.value = true;
  } else {
    isContentEmpty.value = false;
  }
};

const submit = async () => {
  isSubmitting.value = true;
  if (tab.value === 1) {
    await checkContent();
    if (isContentEmpty.value) {
      isSubmitting.value = false;
      return;
    }
    await submitLanguage();
  } else {
    await uploadBannerImage();
    await submitDetail();
  }
  isSubmitting.value = false;
};

const uploadBannerImage = async () => {
  if (formData.value.type == EventShopItemTypes.Product) return;
  await uploadBannerRef.value?.uploadImage().then(() => {
    if (uploadBannerRef.value?.imageUrls.length > 0) {
      formData.value.configuration.bannerImages =
        uploadBannerRef.value?.imageUrls;
    }
  });
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

    await uploadImageRef.value?.uploadImage().then(() => {
      if (uploadImageRef.value?.imageUrls.length > 0) {
        languageFormData.value.images = uploadImageRef.value?.imageUrls;
        showImageError.value = false;
      }
    });
    await EventServices.updateItemLanguage(
      formData.value.id,
      langItem.value,
      languageFormData.value
    );
    languageExists(langItem.value);
    MsgPrompt.success(langItem.value + " updated successfully");
  } catch (error) {
    console.log(error);
    MsgPrompt.error(langItem.value + " update failed");
  }
  emits("submit");
};

const submitDetail = async () => {
  try {
    formData.value.point = new Decimal(pointsInDollar.value)
      .times(10000)
      .toNumber();
    await EventServices.updateItem(formData.value.id, formData.value);
    MsgPrompt.success("Item updated successfully");
  } catch (error) {
    console.log(error);
    MsgPrompt.error("Item update failed");
  }
  emits("submit");
};

const hide = () => {
  modalRef.value?.hide();
  formData.value = {
    point: 1,
    name: "",
    type: EventShopItemTypes.Product,
    category: EventShopItemCategory.bcrMerch,
    accessRoles: ["Client", "IB", "Sales"],
    description: "",
    language: "en-us",
    configuration: {
      validPeriodInDays: 0,
      rewardType: EventShopRewardTypes.Point1000,
    },
  };
  languageFormData.value = {};
  showImageError.value = false;
  contentList.value = {};
  imageList.value = [];
  if (uploadImageRef.value) {
    uploadImageRef.value.imageUrl = "";
  }
  if (uploadBannerRef.value) {
    uploadBannerRef.value.imageUrl = "";
  }

  langItem.value = "";
  pointsInDollar.value = 0;
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
