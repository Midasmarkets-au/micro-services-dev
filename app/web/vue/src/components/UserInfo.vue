<template>
  <div class="d-flex align-items-center text-start">
    <UserAvatar
      @click="onUserClicked"
      :avatar="props.user?.avatar"
      :name="displayName"
      class="me-3 cursor-pointer"
      :side="props.side"
      :rounded="props.rounded"
      :size="props.size"
    />
    <div>
      <!--        @click="() => openUserDetailSider?(user.partyId)"-->
      <a href="#" class="fs-6 text-gray-800 text-hover-primary fw-bold">
        <span
          @click="onUserClicked"
          :class="
            props.user?.partyTags?.includes(TagNames.Special)
              ? 'special-tag'
              : ''
          "
          >{{ displayName }}</span
        >
        <span
          v-if="!disableCommentView && $can('Admin')"
          class="ms-4 cursor-pointer"
          role="button"
          @click="
            onCommentClicked(
              CommentType.User,
              props.user?.partyId,
              props.user?.firstName + ' ' + props.user?.lastName
            )
          "
        >
          <i
            v-if="props.user?.hasComment"
            class="fa-regular fa-comment-dots text-primary"
          ></i>
          <i v-else class="fa-regular fa-comment-dots text-secondary"></i>
        </span>
      </a>
      <div
        class="fs-7 text-muted fw-semobold mt-1"
        style="cursor: pointer"
        @click="handleGodModeClick(props.user.partyId)"
      >
        {{ props.user?.email || props?.email || "" }}
        <span v-if="props.isAdmin" class="text-danger">(Admin)</span>
      </div>
    </div>

    <CommentsView
      v-if="!disableCommentView && $can('TenantAdmin')"
      ref="commentsRef"
      type=""
      id="0"
    />
  </div>
</template>

<script lang="ts" setup>
import { computed, inject, ref } from "vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import { TagNames } from "@/core/types/TagTypes";

const props = withDefaults(
  defineProps<{
    side?: string;
    rounded?: boolean;
    size?: string;
    isAdmin?: boolean;
    user?: any;
    partyId?: number;
    name?: string;
    email?: string;
    disableCommentView?: boolean;
  }>(),
  {
    rounded: false,
    disableCommentView: false,
  }
);
const commentsRef = ref<any>(null);

const displayName = computed(
  () =>
    props?.name ||
    props.user?.nativeName ||
    props.user?.displayName ||
    `${props.user?.firstName} ${props.user?.lastName}`
);
const openUserDetailSider = inject<(partyId: number) => any>(
  InjectKeys.OPEN_USER_DETAILS,
  () => null
);

const onUserClicked = () => {
  if (props.user?.partyId) {
    openUserDetailSider?.(props.user.partyId);
  } else if (props.partyId) {
    openUserDetailSider?.(props.partyId);
  }
};

const openCommentView = inject<
  (type: CommentType, id: number, title: string) => any
>(InjectKeys.OPEN_COMMENT_VIEW, () => null);

const onCommentClicked = (type: CommentType, id: number, title: string) => {
  if (props.user?.partyId) {
    openCommentView?.(type, id, title);
  }
};
const handleGodModeClick = UserService.generateGodModeHandler();

// const viewComments = (type: CommentType, id: number, title: string) => {
//   commentsRef.value?.show(type, id, title);
// };

/*
{
  "firstName": "string",
  "lastName": "string",
  "priorName": "string",
  "birthdate": "string",
  "gender": "string",
  "citizen": "string",
  "ccc": "string",
  "phone": "string",
  "email": "string",
  "address": "string",
  "idFrom": "string",
  "idNumber": "string",
  "idOffice": "string",
  "idIssueDate": "string",
  "idExpiryDate": "string"
}


{
  "nativeName": "string",
  "firstName": "string",
  "lastName": "string",
  "language": "string",
  "timeZone": "string",
  "referCode": "string",
  "countryCode": "string",
  "currency": "string",
  "ccc": "string",
  "birthday": "2023-08-03",
  "gender": 0,
  "citizen": "string",
  "address": "string",
  "idType": 0,
  "idNumber": "string",
  "idIssuer": "string",
  "idIssuedOn": "2023-08-03",
  "idExpireOn": "2023-08-03"
}
 */
</script>
<style scoped lang="scss">
.special-tag {
  color: #db1515;
}
</style>
