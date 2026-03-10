<template>
  <SimpleForm
    ref="modalRef"
    :title="type === 'create' ? 'Add Item' : 'Edit Item'"
    :is-loading="isSubmitting"
    :submit="submit"
    :before-close="hide"
    class="w-1000px"
  >
    <div class="px-5">
      <div>
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-position="top"
        >
          <el-form-item label="Item Name" prop="name">
            <el-input v-model="formData.name" />
          </el-form-item>

          <div class="row row-cols-2 g-5">
            <el-form-item label="Points" prop="points">
              <el-input-number v-model="formData.points" :min="1" />
            </el-form-item>
            <el-form-item label="Quantity" prop="quantity">
              <el-input-number v-model="formData.quantity" />
            </el-form-item>
          </div>
          <el-form-item label="Upload Image" prop="image">
            <UploadImage ref="uploadImageRef" />
          </el-form-item>
          <el-form-item>
            <el-checkbox-group v-model="formData.availableSites">
              <el-checkbox-button
                v-for="site in ConfigSiteTypesSelections"
                :key="site.value"
                :label="site.value"
                >{{ site.label }}</el-checkbox-button
              >
            </el-checkbox-group>
          </el-form-item>

          <el-form-item label="Publish">
            <el-switch v-model="formData.published">Publish</el-switch>
          </el-form-item>
        </el-form>

        <div>
          <p class="fs-6">{{ $t("fields.content") }}</p>
          <div class="input-group w-400px mb-4">
            <select
              class="form-select"
              v-model="langItem"
              :disabled="!canAddLang"
            >
              <option
                v-for="item in langOptions"
                :key="item.code"
                :value="item.code"
                :label="item.code + '   (' + item.name + ')'"
              />
            </select>
            <button
              class="btn btn-success"
              @click="addLang()"
              :disabled="!canAddLang"
            >
              {{ $t("action.addALanguage") }}
            </button>
          </div>
          <el-tabs>
            <el-tab-pane
              :label="index"
              v-for="(item, index) in contentList"
              :key="index"
            >
              <div>
                <quill-editor
                  :key="index"
                  theme="snow"
                  v-model:content="contentList[index]"
                  :modules="getModules(index)"
                  contentType="html"
                  toolbar="full"
                />
              </div>
            </el-tab-pane>
          </el-tabs>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import type { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import type { UploadProps } from "element-plus";
import { ElMessage, Instance } from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";
import ShopServices from "../services/ShopServices";
import { QuillEditor } from "@vueup/vue-quill";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import ImageUploader from "quill-image-uploader";
import BlotFormatter from "quill-blot-formatter/dist/BlotFormatter";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import UploadImage from "@/projects/tenant/components/UploadImage.vue";
const emits = defineEmits<{
  (e: "submit"): void;
}>();
const isSubmitting = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const type = ref("");
const langOptions = ref<any>([]);
const langItem = ref<any>("");
const contentList = ref({});
const uploadImageRef = ref(null);
const hide = () => {
  formData.value = {
    itemId: 0,
    quantity: 1,
    points: 1,
  };
  modalRef.value?.hide();
};

const uploadImage = async (file: any) => {
  const formData = new FormData();
  formData.append("file", file.file);
  const res = await ShopServices.uploadImage(formData);
  return res.data;
};

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  name: [
    { required: true, message: "Please input item name", trigger: "blur" },
  ],
  quantity: [
    { required: true, message: "Please input quantity", trigger: "blur" },
  ],
  description: [
    {
      required: true,
      message: "Please input item description",
      trigger: "blur",
    },
  ],
});
const formData = ref<any>({
  itemId: 0,
  quantity: 1,
  points: 1,
});

const show = async (_type: string, _item: any) => {
  if (_item != null) {
    formData.value = _item;
  }
  type.value = _type;
  modalRef.value?.show();
};

const populateLangOptions = () => {
  const langKeys = Object.keys(contentList.value);
  langOptions.value = LanguageTypes.all.filter(
    (lang) => !langKeys.includes(lang.code)
  );

  if (langOptions.value.length > 0) {
    langItem.value = langOptions.value[0].code;
  }
  // if (langOptions.value.length === LanguageTypes.all.length) {
  //   addLang();
  // }
};

const addLang = () => {
  if (Object.keys(contentList.value).length < LanguageTypes.all.length) {
    contentList.value[langItem.value] = "content " + langItem.value;
    populateLangOptions();
  }
};

const canAddLang = computed(() => {
  return Object.keys(contentList.value).length < LanguageTypes.all.length;
});

const submit = async () => {
  console.log(formData.value);
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  try {
    if (type.value === "create") {
      // const res = await ShopService.createItem(formData.value);
      // MsgPrompt.success("Item created successfully");
    } else {
      // const res = await ShopService.updateItem(formData.value);
      // MsgPrompt.success("Item updated successfully");
    }
  } catch (error) {
    console.log(error);
  }
  emits("submit");
};

const handleAvatarSuccess: UploadProps["onSuccess"] = (
  response,
  uploadFile
) => {
  formData.value.image = URL.createObjectURL(uploadFile.raw!);
};

const beforeAvatarUpload: UploadProps["beforeUpload"] = (rawFile) => {
  if (rawFile.type !== "image/jpeg" && rawFile.type !== "image/png") {
    ElMessage.error("Avatar picture must be JPG format!");
    return false;
  } else if (rawFile.size / 1024 / 1024 > 2) {
    ElMessage.error("Avatar picture size can not exceed 2MB!");
    return false;
  }
  return true;
};

const uploadImg = async (file: any) => {
  const formData = new FormData();
  formData.append("file", file);
  const res = await ShopServices.uploadImage(formData);
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
