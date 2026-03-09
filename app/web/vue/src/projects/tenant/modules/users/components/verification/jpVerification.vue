<template>
  <div class="d-flex flex-column flex-column-fluid">
    <jpHeader />
    <div class="card">
      <div class="card-body py-4">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>{{ $t("fields.verificationId") }}</th>
              <th>{{ $t("fields.client") }}</th>
              <th>{{ $t("fields.status") }}</th>
              <th>{{ $t("fields.updatedOn") }}</th>
              <th
                v-if="
                  criteria.status === VerificationStatusTypes.Approved ||
                  criteria.status == null
                "
              >
                {{ $t("fields.operator") }}
              </th>
              <th class="text-center min-w-150px">
                {{ $t("action.action") }}
              </th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && items.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in items" :key="index">
              <td>
                {{ item.id }}
                <span
                  v-if="$can('Admin')"
                  class="ms-2 cursor-pointer"
                  role="button"
                  @click="
                    openCommentView(
                      CommentType.Verification,
                      item.id,
                      item.user?.firstName + ' ' + item.user?.lastName
                    )
                  "
                >
                  <i
                    v-if="item.hasComment"
                    class="fa-regular fa-comment-dots text-primary"
                  ></i>
                  <i
                    v-else
                    class="fa-regular fa-comment-dots text-secondary"
                  ></i>
                </span>
              </td>
              <td class="d-flex align-items-center">
                <UserInfo :user="item.user" class="me-2" />
              </td>
              <td>
                {{ $t(`status.${VerificationStatus[item.status]}`) }}
              </td>
              <td>
                <TimeShow :date-iso-string="item.updatedOn" />
              </td>
              <td
                v-if="
                  criteria.status == null ||
                  item.status === VerificationStatusTypes.Approved
                "
              >
                <UserInfo
                  v-if="item.operator"
                  :user="item.operator"
                  :disableCommentView="true"
                />
              </td>
              <td class="text-center">
                <button
                  v-if="item.status != VerificationStatusTypes.Incomplete"
                  class="btn btn-light btn-primary btn-sm me-3"
                  @click="show(item.id)"
                >
                  {{ $t("action.details") }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
  </div>
  <UserVerifyDocModal ref="userVerifyDocModal" @user-approved="fetchData(1)" />
  <jpVerifyModal ref="modalRef" @fetch-data="fetchData(1)" />
</template>
<script setup lang="ts">
import { ref, onMounted, inject, provide } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import {
  VerificationStatusTypes,
  VerificationStatus,
} from "@/core/types/VerificationInfos";
import UserService from "../../services/UserService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserVerifyDocModal from "@/projects/tenant/modules/accounts/components/modal/UserVerifyDocModal.vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import { CommentType } from "@/core/types/CommentType";
import jpHeader from "./jpComp/jpHeader.vue";
import jpVerifyModal from "./jpComp/jpVerifyModal.vue";

const userVerifyDocModal = ref<InstanceType<typeof UserVerifyDocModal>>();
const modalRef = ref<any>(null);
const isLoading = ref(true);
const items = ref(Array<any>());
const tab = ref(VerificationStatusTypes.AwaitingReview);
// 74f9f9d
const criteria = ref<any>({
  page: 1,
  size: 10,
  status: tab.value,
  sortField: "UpdatedOn",
});

const openCommentView = inject<
  (type: CommentType, id: number, title: string) => any
>(InjectKeys.OPEN_COMMENT_VIEW, () => null);

const reset = () => {
  criteria.value.email = null;
  fetchData(1);
};

const search = () => {
  criteria.value.status = null;
  fetchData(1);
};

const fetchData = async (selectedPage: number) => {
  criteria.value.page = selectedPage;
  isLoading.value = true;
  try {
    const res = await UserService.queryVerifications(criteria.value);
    items.value = res.data.map((x) => ({
      ...x,
      user: { ...x.user, partyId: x.partyId },
    }));
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

provide("isLoading", isLoading);
provide("criteria", criteria);
provide("tab", tab);
provide("reset", reset);
provide("search", search);
provide("fetchData", fetchData);

// on mounted, fetch the first page of Awaiting verification users
onMounted(() => {
  fetchData(1);
});

const show = (id) => {
  modalRef.value?.show(id);
};

// const pageChange = (page: number) => {
//   fetchData(page);
// };
</script>
