/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.up = function(knex) {
    // Start a transaction to ensure all schema changes are atomic
    await knex.transaction(async (trx) => {
        // Add is_deleted column to product_size table if it doesn't exist
        if (!(await trx.schema.hasColumn('product_size', 'is_deleted'))) {
            await trx.schema.alterTable('product_size', function (table) {
                table
                    .boolean('is_deleted')
                    .notNullable()
                    .defaultTo(false)
                    .comment('Trạng thái xóa màu sắc, mặc định là chưa (false)');
            });
        }

        // Add is_deleted column to product_color table if it doesn't exist
        if (!(await trx.schema.hasColumn('product_color', 'is_deleted'))) {
            await trx.schema.alterTable('product_color', function (table) {
                table
                    .boolean('is_deleted')
                    .notNullable()
                    .defaultTo(false)
                    .comment('Trạng thái xóa kích thước, mặc định là chưa (false)');
            });
        }
    });
};

/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.down = function(knex) {
    // Start a transaction to ensure all schema changes are atomic
    await knex.transaction(async (trx) => {
        // Remove is_deleted column from product_size table if it exists
        if (await trx.schema.hasColumn('product_size', 'is_deleted')) {
            await trx.schema.alterTable('product_size', function (table) {
                table.dropColumn('is_deleted');
            });
        }

        // Remove is_deleted column from product_color table if it exists
        if (await trx.schema.hasColumn('product_color', 'is_deleted')) {
            await trx.schema.alterTable('product_color', function (table) {
                table.dropColumn('is_deleted');
            });
        }
    });
};
