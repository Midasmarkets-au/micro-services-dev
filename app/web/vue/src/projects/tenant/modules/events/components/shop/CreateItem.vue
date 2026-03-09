<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('action.addItem')"
    :is-loading="isSubmitting"
    :submit="submit"
    :before-close="hide"
    class="w-1000px"
  >
    <el-tabs v-model="formData.type" type="card" class="demo-tabs">
      <el-tab-pane
        v-for="(item, index) in EventShopItemOptions"
        :key="index"
        :label="item.label"
        :name="item.value"
        :disabled="isSubmitting"
      ></el-tab-pane>
    </el-tabs>
    <div class="px-5">
      <div v-if="formData.type == EventShopItemTypes.Product">
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-position="top"
        >
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('fields.itemTitle')" prop="title">
              <el-input v-model="formData.title" :disabled="isSubmitting" />
            </el-form-item>
          </div>
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('fields.points')" prop="point">
              <el-input-number
                v-model="formData.point"
                :min="1"
                :disabled="isSubmitting"
              />
            </el-form-item>
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
          </div>
          <div class="row row-cols-2 g-5">
            <div>
              <el-form-item
                :label="$t('fields.category')"
                prop="category"
                v-if="formData.type === EventShopItemTypes.Product"
              >
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
              <el-form-item :label="$t('fields.eventKey')" prop="eventId">
                <el-select
                  v-model="formData.eventId"
                  class="w-200px"
                  :disabled="isSubmitting"
                >
                  <el-option
                    v-for="item in eventOptions"
                    :key="item.id"
                    :label="item.key"
                    :value="item.id"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
            </div>
            <div>
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
              <el-form-item :label="$t('action.uploadImage')" prop="image">
                <UploadImage ref="uploadImageRef" />
              </el-form-item>
            </div>
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
        </el-form>
      </div>
      <div v-if="formData.type != EventShopItemTypes.Product">
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-position="top"
        >
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('fields.rewardName')" prop="name">
              <el-input v-model="formData.name" :disabled="isSubmitting" />
            </el-form-item>
            <el-form-item :label="$t('fields.rewardTitle')" prop="title">
              <el-input v-model="formData.title" :disabled="isSubmitting" />
            </el-form-item>
          </div>
          <div class="row row-cols-2 g-5">
            <el-form-item :label="$t('fields.points')" prop="point">
              <el-input-number
                v-model="formData.point"
                :min="1"
                :disabled="isSubmitting"
              />
            </el-form-item>
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
          </div>
          <div class="row row-cols-2 g-5">
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
          </div>
          <div class="row row-cols-2 g-5">
            <div>
              <el-form-item :label="$t('fields.validDays')">
                <el-input-number
                  v-model="formData.configuration.validPeriodInDays"
                  :disabled="isSubmitting"
                />
              </el-form-item>

              <el-form-item :label="$t('fields.eventKey')" prop="eventId">
                <el-select
                  v-model="formData.eventId"
                  class="w-200px"
                  :disabled="isSubmitting"
                >
                  <el-option
                    v-for="item in eventOptions"
                    :key="item.id"
                    :label="item.key"
                    :value="item.id"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
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
                  >
                    {{ site.label }}
                  </el-checkbox>
                </el-checkbox-group>
              </el-form-item>
              <div class="d-flex gap-4">
                <el-form-item
                  :label="$t('action.uploadCoverImage')"
                  prop="image"
                >
                  <UploadImage ref="uploadImageRef" />
                </el-form-item>
                <el-form-item
                  :label="$t('action.uploadBannerImage')"
                  prop="image"
                >
                  <UploadImage ref="uploadBannerRef" />
                </el-form-item>
              </div>
            </div>
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
        </el-form>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
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
  EventShopItemTypes,
  EventShopItemOptions,
  RewardTypesOptions,
  EventShopRewardTypes,
  RewardRolesOptions,
} from "@/core/types/shop/ShopItemTypes";
import UploadImage from "./UploadImage.vue";
import { AccessRoles } from "@/core/types/shop/ShopEventRoles";
import Decimal from "decimal.js";
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
const langOptions = ref<any>([]);
const contentList = ref({});
const uploadImageRef = ref(null);
const uploadBannerRef = ref(null);
const eventOptions = ref(<any>[]);
const formData = ref<any>({
  point: 1,
  name: "",
  type: EventShopItemTypes.Product,
  category: 0,
  accessRoles: ["Client", "IB", "Sales"],
  accessSites: [0],
  description: "",
  language: "en-us",
  configuration: {
    validPeriodInDays: 0,
    rewardType: EventShopRewardTypes.Point1000,
  },
});
const hide = () => {
  modalRef.value?.hide();
};

const successHide = () => {
  formData.value = {
    point: 1,
    name: "",
    type: EventShopItemTypes.Product,
    category: 0,
    accessRoles: ["Client", "IB", "Sales"],
    accessSites: [0],
    description: "",
    language: "en-us",
    configuration: {
      validPeriodInDays: 0,
      rewardType: EventShopRewardTypes.Point1000,
    },
  };
  uploadImageRef.value.handleRemove();
  modalRef.value?.hide();
};

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  name: [
    { required: true, message: "Please input item name", trigger: "blur" },
  ],
  title: [
    { required: true, message: "Please input item title", trigger: "blur" },
  ],
  eventId: [
    { required: true, message: "Please select event", trigger: "change" },
  ],
});

const accessRoles = AccessRoles;

const show = async (_eventOptions: any) => {
  eventOptions.value = _eventOptions;
  modalRef.value?.show();
};

const populateLangOptions = () => {
  const langKeys = Object.keys(contentList.value);
  langOptions.value = LanguageTypes.all.filter(
    (lang) => !langKeys.includes(lang.code)
  );
};

const submit = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  try {
    isSubmitting.value = true;
    formData.value.name = formData.value.title;
    await uploadBannerRef.value?.uploadImage().then(() => {
      formData.value.configuration.bannerImages =
        uploadBannerRef.value?.imageUrls;
    });
    await uploadImageRef.value
      ?.uploadImage()
      .then(() => {
        formData.value.images = uploadImageRef.value?.imageUrls;
      })
      .then(() => {
        (formData.value.point = new Decimal(formData.value.point)
          .times(10000)
          .toNumber()),
          EventServices.createShopItem(formData.value)
            .then(() => {
              MsgPrompt.success("Item created successfully");
              emits("submit");
            })
            .then(() => {
              successHide();
            });
      });
  } catch (error) {
    console.log(error);
  }
  isSubmitting.value = false;
  emits("submit");
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
