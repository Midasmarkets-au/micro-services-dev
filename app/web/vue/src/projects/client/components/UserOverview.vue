<template>
  <div class="card mb-5 mb-xl-2 position-relative">
    <div v-if="isMobile">
      <div class="position-absolute" style="right: 5px; top: 5px">
        <router-link to="/profile" class="btn btn-sm btn-icon white">
          <span class="svg-icon svg-icon-2">
            <inline-svg src="/images/icons/general/gen063.svg" />
          </span>
        </router-link>
      </div>
      <!--    Mobile start -->
      <div class="card-body">
        <div class="text-center">
          <div class="mb-3">
            <UserAvatar
              :avatar="user.avatar"
              :name="user.nativeName"
              side="client"
              size="64px"
              class="mx-auto"
              rounded
            ></UserAvatar>
          </div>

          <div class="ms-5">
            <router-link
              to="/profile"
              class="text-gray-800 fw-bold text-hover-primary fs-3"
            >
              {{ user.name }}
            </router-link>
            <span class="text-muted d-block fw-semibold fs-5">
              <span class="svg-icon svg-icon-3 me-1"
                ><inline-svg src="/images/icons/communication/com011.svg"
              /></span>
              <span>{{ user.email }}</span>
            </span>
          </div>
        </div>
      </div>
    </div>
    <!--    Mobile end -->

    <div
      v-if="!isMobile"
      class="card-body pt-10 px-0 pb-0 position-relative user-overview"
    >
      <div class="user-card-setting">
        <router-link to="/profile" class="btn btn-sm btn-icon white">
          <span class="svg-icon svg-icon-1">
            <inline-svg src="/images/icons/general/gen063.svg" />
          </span>
        </router-link>
      </div>
      <div class="d-flex flex-column text-center mb-12 px-9 pt-9">
        <div class="d-flex justify-content-center mb-6">
          <UserAvatar
            :avatar="user.avatar"
            :name="user?.nativeName || user?.name"
            side="client"
            size="117px"
            rounded
          ></UserAvatar>
        </div>
        <div class="text-center mt-3 mb-4">
          <router-link
            to="/profile"
            class="fw-bold text-hover-primary"
            style="font-size: 24px"
          >
            {{ user.nativeName || user.name }}
          </router-link>
          <div
            class="text-muted mt-5 fw-semibold gap-1"
            style="font-size: 18px"
          >
            <!-- <span class="svg-icon svg-icon-3"
              ><inline-svg src="/images/icons/communication/com011.svg"
            /></span> -->
            <span class="user_mail"> {{ user.email }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { useStore } from "@/store";
import { computed } from "vue";
import { isMobile } from "@/core/config/WindowConfig";

const store = useStore();
const user = computed<any>(() => store.state.AuthModule.user);
</script>

<style>
.user-card-setting {
  position: absolute;
  top: 10px;
  right: 10px;
}

.role-badge {
  padding: 2px 16px;
  font-weight: 500;
  border-radius: 8px;
}
.user_mail {
  position: relative;
  padding-left: 20px;
  &::after {
    content: "";
    position: absolute;
    top: 5px;
    left: 0;
    width: 22px;
    height: 22px;
    background-image: url("/images/icons/communication/com011.svg");
    background-repeat: no-repeat;
    border-radius: 8px;
  }
}
.user-overview {
  max-height: 358px;
}
</style>
