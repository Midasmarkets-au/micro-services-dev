<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div class="d-flex gap-10 justify-content-between">
      <CommonTabs />
      <div>
        <el-input
          class="w-400px"
          v-model="criteria.searchText"
          @keyup.enter="search"
          :clearable="true"
          placeholder="Search Key Words"
        >
          <template #append>
            <el-button :icon="Search" @click="search" />
          </template>
        </el-input>
        <el-button class="ms-4" type="info" plain @click="reset">{{
          $t("action.reset")
        }}</el-button>
      </div>
    </div>
    <div class="card">
      <div
        class="text-end w-100 mb-4 d-flex align-items-center justify-content-end gap-1 pt-7 pe-7"
      >
        <div class="d-flex align-items-center">
          <el-icon color="#FF0000"><WarningFilled /></el-icon>
          {{ $t("title.cheaterIp") }} |
        </div>
        <div class="d-flex align-items-center">
          <el-icon><Avatar /></el-icon> {{ $t("status.blocked") }}
        </div>
      </div>

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
                  <el-icon
                    v-if="item.user.isInIpBlackList"
                    class="ms-1"
                    color="#FF0000"
                    ><WarningFilled
                  /></el-icon>
                  <el-icon v-if="item.user.isInUserBlackList" class="ms-1"
                    ><Avatar
                  /></el-icon>
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
                  disableCommentView="true"
                />
              </td>
              <td class="text-center">
                <button
                  v-if="item.status != VerificationStatusTypes.Incomplete"
                  class="btn btn-light btn-primary btn-sm me-3"
                  @click="userVerifyDocModal?.show(item.id)"
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
  <UserVerifyDocModal
    ref="userVerifyDocModal"
    @user-approved="onVerificationFinished"
    @user-moved="fetchData(1)"
  />
</template>
<script setup lang="ts">
import { ref, onMounted, watch, inject, provide } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import {
  VerificationStatusTypes,
  VerificationStatus,
} from "@/core/types/VerificationInfos";
import UserService from "../../services/UserService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserVerifyDocModal from "@/projects/tenant/modules/accounts/components/modal/UserVerifyDocModal.vue";
import { Search, WarningFilled, Avatar } from "@element-plus/icons-vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import CommonTabs from "@/projects/tenant/components/CommonTabs.vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const userVerifyDocModal = ref<InstanceType<typeof UserVerifyDocModal>>();
const isLoading = ref(true);
const items = ref(Array<any>());
const tab = ref(VerificationStatusTypes.AwaitingReview);
// 74f9f9d
const criteria = ref({
  page: 1,
  size: 10,
  status: tab.value,
  sortField: "UpdatedOn",
});

const tabsData = [
  {
    index: 0,
    label: t("status.pending"),
    status: VerificationStatusTypes.AwaitingReview,
  },
  {
    index: 1,
    label: t("status.reviewing"),
    status: VerificationStatusTypes.UnderReview,
  },
  {
    index: 2,
    label: t("status.reviewed"),
    status: VerificationStatusTypes.AwaitingApprove,
  },
  {
    index: 3,
    label: t("status.approved"),
    status: VerificationStatusTypes.Approved,
  },
  {
    index: 4,
    label: t("status.rejected"),
    status: VerificationStatusTypes.Rejected,
  },
  {
    index: 5,
    label: t("status.incomplete"),
    status: VerificationStatusTypes.Incomplete,
  },
  {
    index: 6,
    label: t("status.all"),
    status: null,
  },
];

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
// on mounted, fetch the first page of Awaiting verification users
onMounted(() => {
  fetchData(1);
});

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

provide("criteria", criteria);
provide("isLoading", isLoading);
provide("tabsData", tabsData);
provide("fetchData", fetchData);

const onVerificationFinished = async () => {
  await fetchData(1);
  userVerifyDocModal.value?.hide();
};

watch(
  () => criteria.value.status,
  () => fetchData(1)
);

// const pageChange = (page: number) => {
//   fetchData(page);
// };
</script>
