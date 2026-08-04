<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('title.materialRequest')"
    width="600"
    align-center
  >
    <div v-loading="isLoading">
      <template v-if="item">
        <div class="d-flex align-items-center justify-content-between mb-5">
          <UserInfo :user="item.user" />
          <span>{{ $t(`status.${VerificationStatus[item.status]}`) }}</span>
        </div>

        <table class="table table-tenant">
          <tbody>
            <tr>
              <th class="w-150px">{{ $t("fields.materialType") }}</th>
              <td>{{ item.content.materialType }}</td>
            </tr>
            <tr>
              <th>{{ $t("fields.quantity") }}</th>
              <td>{{ item.content.quantity ?? "-" }}</td>
            </tr>
            <tr>
              <th>{{ $t("fields.description") }}</th>
              <td style="white-space: pre-wrap">
                {{ item.content.description }}
              </td>
            </tr>
            <tr>
              <th>{{ $t("fields.createdOn") }}</th>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
            </tr>
            <tr v-if="item.content.attachments?.length">
              <th>{{ $t("fields.attachments") }}</th>
              <td>
                <a
                  v-for="(file, index) in item.content.attachments"
                  :key="index"
                  :href="file.url"
                  target="_blank"
                  class="d-block"
                >
                  {{ file.fileName }}
                </a>
              </td>
            </tr>
            <tr v-if="item.note">
              <th>{{ $t("fields.note") }}</th>
              <td>{{ item.note }}</td>
            </tr>
          </tbody>
        </table>

        <el-form v-if="item.status === VerificationStatusTypes.AwaitingReview">
          <el-form-item :label="$t('fields.reason')">
            <el-input
              v-model="rejectNote"
              type="textarea"
              :rows="3"
              :placeholder="$t('fields.reason')"
            />
          </el-form-item>
        </el-form>
      </template>
    </div>
    <template #footer>
      <div
        class="dialog-footer"
        v-if="item && item.status === VerificationStatusTypes.AwaitingReview"
      >
        <el-button @click="dialogRef = false" :disabled="isSubmitting">
          {{ $t("action.close") }}
        </el-button>
        <el-button type="danger" @click="reject" :loading="isSubmitting">
          {{ $t("action.reject") }}
        </el-button>
        <el-button type="success" @click="approve" :loading="isSubmitting">
          {{ $t("action.approve") }}
        </el-button>
      </div>
      <div class="dialog-footer" v-else>
        <el-button @click="dialogRef = false">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import MaterialRequestService from "../../services/MaterialRequestService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TimeShow from "@/components/TimeShow.vue";
import {
  VerificationStatusTypes,
  VerificationStatus,
} from "@/core/types/VerificationInfos";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const dialogRef = ref(false);
const isLoading = ref(false);
const isSubmitting = ref(false);
const item = ref<any>(null);
const rejectNote = ref("");

const emits = defineEmits<{
  (e: "updated"): void;
}>();

const show = async (id: number) => {
  dialogRef.value = true;
  isLoading.value = true;
  item.value = null;
  rejectNote.value = "";
  try {
    item.value = await MaterialRequestService.getMaterialRequest(id);
  } catch (error) {
    console.error(error);
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const approve = async () => {
  if (!item.value) return;
  isSubmitting.value = true;
  try {
    await MaterialRequestService.approveMaterialRequest(item.value.id);
    MsgPrompt.success();
    dialogRef.value = false;
    emits("updated");
  } catch (error) {
    console.error(error);
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
};

const reject = async () => {
  if (!item.value) return;
  if (!rejectNote.value) {
    MsgPrompt.warning(t("fields.reason"));
    return;
  }
  isSubmitting.value = true;
  try {
    await MaterialRequestService.rejectMaterialRequest(
      item.value.id,
      rejectNote.value
    );
    MsgPrompt.success();
    dialogRef.value = false;
    emits("updated");
  } catch (error) {
    console.error(error);
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
};

defineExpose({
  show,
});
</script>
