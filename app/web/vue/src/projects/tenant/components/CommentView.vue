<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="commentsViewModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-650px">
      <div class="modal-content" v-if="viewCommentId > 0">
        <div class="modal-header">
          <h2 class="fw-bold">
            {{ $t("title.commentsView") }}
            <span
              v-if="
                viewCommentType == CommentType.Deposit ||
                viewCommentType == CommentType.Withdrawal ||
                viewCommentType == CommentType.Transfer
              "
              class="text-primary"
              >{{ $t("type.comment." + viewCommentType) }}:
              {{ viewCommentTitle }}
            </span>

            <span v-else class="text-primary"
              >{{ $t("type.comment." + viewCommentType) }}:
              {{ viewCommentTitle }} - {{ viewCommentId }}
            </span>
          </h2>
          <div
            class="btn btn-icon btn-sm btn-active-icon-primary"
            id="kt_modal_add_event_close"
            data-bs-dismiss="modal"
          >
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <div class="modal-body">
          <div class="row">
            <div class="col">
              <div class="input-group input-group-sm mb-3">
                <span class="input-group-text" id="inputGroup-sizing-sm">{{
                  $t("fields.comment")
                }}</span>
                <input
                  type="text"
                  class="form-control"
                  aria-label="Sizing example input"
                  aria-describedby="inputGroup-sizing-sm"
                  v-model="commentContent"
                />
                <button
                  class="btn btn-secondary"
                  type="button"
                  id="button-addon2"
                  :disabled="isLoading || submited"
                  :data-kt-indicator="submited ? 'on' : null"
                  @click="addComment"
                >
                  <span v-if="!submited"
                    >{{ $t("action.add") }} {{ $t("fields.comment") }}</span
                  >
                  <span v-else class="indicator-progress">
                    {{ $t("title.inProgress") }}
                    <span
                      class="spinner-border spinner-border-sm align-middle ms-2"
                    ></span>
                  </span>
                </button>
              </div>
              <div
                id="commentError"
                v-if="commentError != ''"
                class="form-text text-danger"
              >
                {{ commentError }}
              </div>
            </div>
          </div>
          <hr />
          <div class="row">
            <div class="col">
              <table
                class="table align-middle table-row-dashed fs-6 gy-1 table-striped"
                id="table_accounts_requests"
              >
                <thead>
                  <tr
                    class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
                  >
                    <th class="col-7">{{ $t("fields.comment") }}</th>
                    <th class="col-1">{{ $t("fields.user") }}</th>
                    <th class="col-3">{{ $t("fields.createdOn") }}</th>
                    <th class="col-1">{{ $t("action.action") }}</th>
                  </tr>
                </thead>
                <tbody v-if="isLoading">
                  <LoadingRing />
                </tbody>
                <tbody v-else-if="!isLoading && comments.length === 0">
                  <NoDataBox />
                </tbody>

                <tbody v-else class="text-gray-600">
                  <tr v-for="(item, index) in comments" :key="index">
                    <td>{{ item.content }}</td>
                    <td>{{ item.operatorName }}</td>
                    <td><TimeShow :date-iso-string="item.createdOn" /></td>
                    <td>
                      <button
                        class="btn btn-sm btn-danger p-1"
                        :data-kt-indicator="submited ? 'on' : null"
                        :disabled="isLoading || submited"
                        @click="deleteComment(item.id, index)"
                      >
                        <span v-if="!submited">{{ $t("action.delete") }}</span>
                        <span v-else class="indicator-progress">
                          <span
                            class="spinner-border spinner-border-sm align-middle ms-2"
                          ></span>
                        </span>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import { CommentType } from "@/core/types/CommentType";
import GlobalService from "../services/TenantGlobalService";

const commentsViewModalRef = ref<null | HTMLElement>(null);

const isLoading = ref(true);
const submited = ref(false);

const viewCommentType = ref<CommentType>(0);
const viewCommentId = ref(0);
const viewCommentTitle = ref("");

const comments = ref([]);
const commentContent = ref("");
const commentError = ref("");

const show = (type: CommentType, id, title: string) => {
  comments.value = [];
  viewCommentType.value = type;
  viewCommentId.value = id;
  viewCommentTitle.value = title;
  submited.value = false;
  showModal(commentsViewModalRef.value);
  fetchData(type, id);
};

const fetchData = async () => {
  comments.value = (
    await GlobalService.queryComments({
      type: viewCommentType.value,
      size: 100,
      rowId: viewCommentId.value,
    })
  ).data;
};

const addComment = async () => {
  submited.value = true;
  if (commentContent.value == "") {
    commentError.value = "Content is required!";
    return;
  }
  commentError.value = "";
  let data = await GlobalService.createComment({
    type: viewCommentType.value,
    rowid: viewCommentId.value,
    content: commentContent.value,
  });

  comments.value.unshift(data);
  commentContent.value = "";
  submited.value = false;
};

const deleteComment = async (commentId, index) => {
  submited.value = true;
  await GlobalService.deleteComment(commentId, {
    type: viewCommentType.value,
  });

  comments.value.splice(index, 1);
  submited.value = false;
};

const hide = () => {
  hideModal(commentsViewModalRef.value);
};

onMounted(async () => {
  isLoading.value = true;
  isLoading.value = false;
});

defineExpose({
  show,
  hide,
});
</script>
