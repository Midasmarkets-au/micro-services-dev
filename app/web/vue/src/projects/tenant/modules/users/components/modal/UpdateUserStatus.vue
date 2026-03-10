<template>
  <div>
    <SimpleForm
      ref="modalForm"
      :title="$t('fields.updateUserStatus')"
      :is-loading="isLoading"
      disable-footer
    >
      <div class="my-5">
        <div class="d-flex justify-content-center mb-9">
          <el-select
            v-model="status"
            :disabled="isLoading"
            placeholder="Select"
            class="w-250px"
          >
            <el-option
              :label="$t('status.active')"
              :value="PartyStatusTypes.Active"
            />
            <el-option
              :label="$t('status.inactive')"
              :value="PartyStatusTypes.Closed"
            />
          </el-select>
        </div>

        <div class="mb-9">
          <div class="d-flex justify-content-center" :disabled="isLoading">
            <el-input
              v-model="comment"
              class="mb-2 w-250px"
              :placeholder="$t('tip.enterReason')"
              type="textarea"
            />
          </div>
          <p class="text-danger text-center" v-if="showCommentError">
            {{ $t("error.fieldIsRequired") }}
          </p>
        </div>

        <div class="d-flex justify-content-center">
          <button
            class="btn btn-light btn-success btn-md me-3"
            @click="updateStatus"
          >
            <span v-if="isLoading">
              {{ $t("action.waiting") }}
              <span
                class="spinner-border h-15px w-15px align-middle text-gray-400"
              ></span>
            </span>
            <span v-else>{{ $t("action.update") }}</span>
          </button>
        </div>
      </div>
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { ref, inject } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import UserService from "../../services/UserService";
import { PartyStatusTypes } from "@/core/types/PartyStatusTypes";
import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const isLoading = ref(false);
const modalForm = ref<InstanceType<typeof SimpleForm>>();
const userInfos = inject<any>("userInfos");
const comment = ref("");
const status = ref(userInfos.value.status);
const showCommentError = ref(false);

const updateStatus = async () => {
  if (!comment.value || comment.value.trim() === "") {
    showCommentError.value = true;
    return;
  }
  showCommentError.value = false;
  isLoading.value = true;

  try {
    let form = {
      status: status.value,
      comment: comment.value,
    };
    await UserService.updateUserStatus(userInfos.value.partyId, form);
    ElNotification({
      title: t("status.success"),
      message: t("tip.updateSuccess"),
      type: "success",
    });
    userInfos.value.status = status.value;
    modalForm.value?.hide();
  } catch (error) {
    ElNotification({
      title: t("status.failed"),
      message: t("tip.updateFailed"),
      type: "error",
    });
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  async show() {
    modalForm.value?.show();
  },
  hide() {
    modalForm.value?.hide();
  },
});
</script>
